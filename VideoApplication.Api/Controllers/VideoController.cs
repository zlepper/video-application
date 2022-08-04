using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using VideoApplication.Api.Database;
using VideoApplication.Api.Exceptions.Channels;
using VideoApplication.Api.Extensions;

namespace VideoApplication.Api.Controllers;

[Authorize]
[AllowAnonymous]
[ApiController]
[Route("api/videos")]
public class VideoController : ControllerBase
{
    private readonly ILogger<VideoController> _logger;
    private readonly VideoApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public VideoController(ILogger<VideoController> logger, VideoApplicationDbContext dbContext, IClock clock)
    {
        _logger = logger;
        _dbContext = dbContext;
        _clock = clock;
    }

    [HttpGet("{channelSlug}")]
    public async Task<List<VideoResponse>> GetChannelVideos([FromRoute] string channelSlug, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting channel '{@Slug}' videos", channelSlug);

        var channel =
            await _dbContext.Channels.FirstOrDefaultAsync(c => c.IdentifierName == channelSlug, cancellationToken);

        if (channel == null)
        {
            throw new ChannelNotFoundException(channelSlug);
        }
        
        var query = _dbContext.Videos.Where(v => v.ChannelId == channel.Id);

        if (User.GetIdOrNull() != channel.OwnerId)
        {
            var now = _clock.GetCurrentInstant();
            query = query.Where(v => v.PublishDate < now);
        }

        return await query
            .OrderBy(v => v.UploadDate)
            .Select(v => new VideoResponse(v.Id, v.Name, v.UploadDate, v.PublishDate))
            .ToListAsync(cancellationToken);
    }
}

public record VideoResponse(Guid Id, string Name, Instant UploadDate, Instant? PublishDate);