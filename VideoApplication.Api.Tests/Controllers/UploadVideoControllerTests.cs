using System.Security.Cryptography;
using NUnit.Framework;
using VideoApplication.Api.Controllers;
using VideoApplication.Api.Services;

namespace VideoApplication.Api.Tests.Controllers;

[TestFixture]
public class UploadVideoControllerTests : TestBase<UploadVideoController>
{
    private const string testFileName = "TestFiles/Sample-MP4-Video-File.mp4";
    
    private async Task<string> GetTestFileHash(string name)
    {
        await using var file = File.OpenRead(name);
        return await HashStream(file);
    }

    private static async Task<string> HashStream(Stream file)
    {
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(file);
        return Convert.ToHexString(hash);
    }

    [Test]
    public async Task UploadsFile()
    {
        var channel = await PrepareTestSystem();
        
        var hash = await GetTestFileHash(testFileName);
        var initiateResponse = await Service.StartVideoUpload(new StartVideoUploadRequest(hash, channel.Id, testFileName,
            new FileInfo(testFileName).Length));
        
        Assert.That(initiateResponse.UploadedChunks, Is.Empty);

        await using (var uploadStream = File.OpenRead(testFileName))
        {
            Service.Request.Body = uploadStream;
            await Service.UploadChunk(initiateResponse.UploadId, 0);
            Service.Request.Body = Stream.Null;
        }

        await Service.FinishUpload(new FinishUploadRequest(initiateResponse.UploadId));

        var storagePath = StorageStructureHelper.GetSourcePath(channel.Id, initiateResponse.UploadId);
        await using var resultStream = await ServiceProvider.GetRequiredService<StorageWrapper>()
            .OpenReadStream(storagePath, CancellationToken.None);

        var uploadedHash = await HashStream(resultStream);
        Assert.That(uploadedHash, Is.EqualTo(hash));
    }
}