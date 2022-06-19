using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Rebus.Bus;
using VideoApplication.Api.Controllers.Channels.Requests;
using VideoApplication.Api.Controllers.Channels.Responses;
using VideoApplication.Api.Database;
using VideoApplication.Api.Database.Models;
using VideoApplication.Api.Exceptions.Channels;
using VideoApplication.Api.Extensions;
using VideoApplication.Api.Shared.Commands;
using VideoApplication.Api.Shared.Events;

namespace VideoApplication.Api.Controllers;

[ApiController]
[Route("api/channels")]
public class ChannelController : ControllerBase
{

    private readonly ILogger<ChannelController> _logger;
    private readonly VideoApplicationDbContext _dbContext;
    private readonly IBus _bus;
    private readonly IClock _clock;

    public ChannelController(ILogger<ChannelController> logger, VideoApplicationDbContext dbContext, IBus bus, IClock clock)
    {
        _logger = logger;
        _dbContext = dbContext;
        _bus = bus;
        _clock = clock;
    }

    [HttpPost]
    [Authorize]
    public async Task<ChannelResponse> CreateChannel(CreateChannelRequest request, CancellationToken cancellationToken = default)
    {
        var channel = new Channel()
        {
            Id = Guid.NewGuid(),
            DisplayName = request.DisplayName,
            IdentifierName = request.IdentifierName,
            Description = request.Description,
            OwnerId = User.GetId(),
            CreatedAt = _clock.GetCurrentInstant()
        };

        _dbContext.Channels.Add(channel);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _bus.Publish(new ChannelCreated(channel.Id, channel.DisplayName, channel.IdentifierName,
                channel.OwnerId));
            return CreateResponse(channel);

        }
        catch (DbUpdateException e) when (e.IsUniqueConstraintViolation())
        {
            _logger.LogError(e, "Channel with identifier name {IdentifierName} or display name {DisplayName} already exists", channel.IdentifierName, channel.DisplayName);
            throw new ChannelAlreadyExistsException();
        }
    }
    
    [HttpGet]
    [Authorize]
    public async Task<List<ChannelResponse>> GetMyChannels(CancellationToken cancellationToken = default)
    {
        var ownerId = User.GetId();

        var channels = await _dbContext.Channels.Where(c => c.OwnerId == ownerId).ToListAsync(cancellationToken);

        return channels.Select(CreateResponse).ToList();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task DeleteChannel(Guid id, CancellationToken cancellationToken = default)
    {
        var channel = await _dbContext.Channels.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (channel == null)
        {
            _logger.LogWarning("Channel with id {Id} does not exist", id);
            return;
        }

        var userId = User.GetId();
        if (channel.OwnerId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to delete channel {ChannelId} they do not own", userId, channel.Id);
            throw new NotChannelOwnerException();
        }

        channel.MarkedForDeletion = true;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _bus.Send(new DeleteChannelAsync(channel.Id));

    }

    private static ChannelResponse CreateResponse(Channel channel)
    {
        return new ChannelResponse(channel.Id, channel.IdentifierName, channel.DisplayName, channel.Description);
    }
}