using Microsoft.EntityFrameworkCore;
using Rebus.TestHelpers.Events;
using VideoApplication.Api.Controllers;
using VideoApplication.Api.Controllers.Channels.Requests;
using VideoApplication.Api.Exceptions.Channels;
using VideoApplication.Api.Shared.Commands;
using VideoApplication.Api.Shared.Events;

namespace VideoApplication.Api.Tests.Controllers;

[TestFixture]
public class ChannelControllerTests : TestBase<ChannelController>
{
    [Test]
    public async Task CanManageChannels()
    {
        var testUser = await CreateTestUser();
        
        SetUserContext(testUser);
        var request = new CreateChannelRequest("myChannel", "My Channel", "This is my channel for testing");

        var channel = await Service.CreateChannel(request);

        Assert.That(channel.Description, Is.EqualTo(request.Description));
        Assert.That(channel.IdentifierName, Is.EqualTo(request.IdentifierName));
        Assert.That(channel.DisplayName, Is.EqualTo(request.DisplayName));

        var messagePublished = Bus!.Events.OfType<MessagePublished<ChannelCreated>>().Single();
        Assert.That(messagePublished.EventMessage, Is.EqualTo(new ChannelCreated(channel.Id, channel.DisplayName, channel.IdentifierName, testUser.UserId)));

        Assert.That(async () =>
        {
            await Service.CreateChannel(request);
        }, Throws.TypeOf<ChannelAlreadyExistsException>());
        
        DbContext.ChangeTracker.Clear();

        var myChannels = await Service.GetMyChannels();
        
        Assert.That(myChannels, Is.EquivalentTo(new []{channel}));

        await Service.DeleteChannel(channel.Id);

        var messageSent = Bus.Events.OfType<MessageSent<DeleteChannelAsync>>().Single();
        Assert.That(messageSent.CommandMessage, Is.EqualTo(new DeleteChannelAsync(channel.Id)));

        var dbChannel = await DbContext.Channels.SingleAsync(c => c.Id == channel.Id);
        Assert.That(dbChannel.MarkedForDeletion, Is.True);
    }
}