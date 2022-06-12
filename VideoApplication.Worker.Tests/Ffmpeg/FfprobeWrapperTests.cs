using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using VideoApplication.Worker.Ffmpeg;

namespace VideoApplication.Worker.Tests.Ffmpeg;

[TestFixture]
// [Timeout(300000)]
public class FfprobeWrapperTests
{
    [Test]
    public async Task GetsVideoInformation()
    {
        await using var services = new ServiceCollection()
            .AddWorkerServices()
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
                new StreamInfo("video-stream-0", StreamType.Video, "h264"),
                new StreamInfo("Desktop", StreamType.Audio, "aac"),
                new StreamInfo("Mic", StreamType.Audio, "aac")
            }));
        });
    }
}