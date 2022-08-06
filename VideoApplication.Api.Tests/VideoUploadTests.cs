using Rebus.Handlers;
using VideoApplication.Api.Controllers;
using VideoApplication.Api.Tests.Controllers;
using VideoApplication.Worker.Shared.Events;

namespace VideoApplication.Api.Tests;

[TestFixture]
[Timeout(120000)]
public class VideoUploadTests : IntegrationTestBase<UploadVideoController>
{
    private TaskCompletionSource _videoTranscodedTask = new TaskCompletionSource();
    
    protected override void AddMoreDependencies(IServiceCollection services)
    {
        services.AddSingleton<IHandleMessages<VideoTranscodingFinished>>(new TranscodeFinishedDetector(_videoTranscodedTask));
        base.AddMoreDependencies(services);
    }

    private async Task<FinishUploadResponse> UploadVideo()
    {
        const string testFileName = TestFileInfo.testFileName;
        
        var channel = await PrepareTestSystem();
        
        var hash = await TestFileInfo.GetTestFileHash(testFileName);
        var initiateResponse = await Service.StartVideoUpload(new StartVideoUploadRequest(hash, channel.Id, testFileName,
            new FileInfo(testFileName).Length));
        
        Assert.That(initiateResponse.UploadedChunks, Is.Empty);

        await using (var uploadStream = File.OpenRead(testFileName))
        {
            var formFile = new FormFile(uploadStream, 0, uploadStream.Length, "chunk", testFileName);

            var chunkRequest = new UploadChunkRequest(initiateResponse.UploadId, 0, hash, formFile);
            await Service.UploadChunk(chunkRequest);
        }

        return await Service.FinishUpload(new FinishUploadRequest(initiateResponse.UploadId));
    }

    [Test]
    public async Task ProcessesVideo()
    {
        var video = await UploadVideo();

        await _videoTranscodedTask.Task;
    }

    internal class TranscodeFinishedDetector : IHandleMessages<VideoTranscodingFinished>
    {
        private readonly TaskCompletionSource _taskCompletionSource;

        public TranscodeFinishedDetector(TaskCompletionSource taskCompletionSource)
        {
            _taskCompletionSource = taskCompletionSource;
        }

        public Task Handle(VideoTranscodingFinished message)
        {
            _taskCompletionSource.SetResult();
            return Task.CompletedTask;
        }
    }
}

