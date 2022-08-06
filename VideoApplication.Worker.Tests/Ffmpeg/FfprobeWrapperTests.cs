using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using NUnit.Framework;
using Rebus.Bus;
using Rebus.TestHelpers;
using VideoApplication.Shared.Storage;
using VideoApplication.Worker.Ffmpeg;

namespace VideoApplication.Worker.Tests.Ffmpeg;

[TestFixture]
[Timeout(30000)]
public class FfprobeWrapperTests
{
    [Test]
    public async Task GetsVideoInformation()
    {
        await using var services = new ServiceCollection()
            .AddWorkerServices()
            .AddSingleton<IHostApplicationLifetime, ApplicationLifetime>()
            .AddSingleton<IBus, FakeBus>()
            .AddS3Storage(new ConfigurationRoot(new List<IConfigurationProvider>()))
            .BuildServiceProvider(new ServiceProviderOptions()
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });

        foreach (var hostedService in services.GetServices<IHostedService>())
        {
            await hostedService.StartAsync(CancellationToken.None);
        }

        var ffprobeWrapper = services.GetRequiredService<FfprobeWrapper>();

        var videoInfo = await ffprobeWrapper.GetVideoInformation("TestFiles/test-video.mkv", CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(videoInfo.Duration, Is.EqualTo(TimeSpan.FromSeconds(10.35)));
            Assert.That(videoInfo.Streams, Is.EquivalentTo(new List<StreamInfo>
            {
                new StreamInfo("video-stream-0", StreamType.Video, "h264", 1920, 1080, 0, 60),
                new StreamInfo("Desktop", StreamType.Audio, "aac", 0, 0, 0, 0),
                new StreamInfo("Mic", StreamType.Audio, "aac", 0, 0, 1, 0)
            }));
        });
    }
}