using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using VideoApplication.Api.Controllers;
using VideoApplication.Api.Services;

namespace VideoApplication.Api.Tests.Controllers;

[TestFixture]
[Parallelizable(ParallelScope.All)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
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
            var formFile = new FormFile(uploadStream, 0, uploadStream.Length, "chunk", testFileName);

            var chunkRequest = new UploadChunkRequest(initiateResponse.UploadId, 0, formFile);
            await Service.UploadChunk(chunkRequest);
        }

        await Service.FinishUpload(new FinishUploadRequest(initiateResponse.UploadId));

        var storagePath = StorageStructureHelper.GetSourcePath(channel.Id, initiateResponse.UploadId);
        await using var resultStream = await ServiceProvider.GetRequiredService<StorageWrapper>()
            .OpenReadStream(storagePath, CancellationToken.None);

        var uploadedHash = await HashStream(resultStream);
        Assert.That(uploadedHash, Is.EqualTo(hash));
        
        Assert.That(DbContext.Uploads, Is.Empty);
        var video = await DbContext.Videos.SingleAsync();
        Assert.That(video.Id, Is.EqualTo(initiateResponse.UploadId));
        Assert.That(video.OriginalFileName, Is.EqualTo(Path.GetFileName(testFileName)));
        Assert.That(video.Name, Is.EqualTo(Path.GetFileNameWithoutExtension(testFileName)));
    }
    
    [TestCase(5 * 1024 * 1024)] // Min size of chunks
    [TestCase(10 * 1024 * 1024)] // Decent size, several chunks
    [TestCase(20 * 1024 * 1024)] // Larger size, still 3 chunks
    [TestCase(30 * 1024 * 1024)] // Only 2 chunks
    public async Task UploadsFileInMultipleChunks(int chunkSize)
    {
        var channel = await PrepareTestSystem();
        
        var hash = await GetTestFileHash(testFileName);
        var initiateResponse = await Service.StartVideoUpload(new StartVideoUploadRequest(hash, channel.Id, testFileName,
            new FileInfo(testFileName).Length));
        
        Assert.That(initiateResponse.UploadedChunks, Is.Empty);

        var fullFileContents = await File.ReadAllBytesAsync(testFileName);

        var iterationCount = 0;
        for (var start = 0; start < fullFileContents.Length; start += chunkSize)
        {
            Assert.That(iterationCount, Is.LessThan(50));
            var end = Math.Min(start + chunkSize, fullFileContents.Length);
            var slice = fullFileContents[start..end];

            await using var memStream = new MemoryStream(slice, false);
            var formFile = new FormFile(memStream, 0, slice.Length, "chunk", testFileName);

            var chunkRequest = new UploadChunkRequest(initiateResponse.UploadId, iterationCount, formFile);
            await Service.UploadChunk(chunkRequest);
            iterationCount++;
        }
        
        await Service.FinishUpload(new FinishUploadRequest(initiateResponse.UploadId));

        var storagePath = StorageStructureHelper.GetSourcePath(channel.Id, initiateResponse.UploadId);
        await using var resultStream = await ServiceProvider.GetRequiredService<StorageWrapper>()
            .OpenReadStream(storagePath, CancellationToken.None);

        var uploadedHash = await HashStream(resultStream);
        Assert.That(uploadedHash, Is.EqualTo(hash));
    }
    
    [Test]
    public async Task CanRestartUpload()
    {
        const int chunkSize = 30 * 1024 * 1024;
        var channel = await PrepareTestSystem();
        
        var hash = await GetTestFileHash(testFileName);
        var startVideoUploadRequest = new StartVideoUploadRequest(hash, channel.Id, testFileName,
            new FileInfo(testFileName).Length);
        var initiateResponse = await Service.StartVideoUpload(startVideoUploadRequest);
        
        Assert.That(initiateResponse.UploadedChunks, Is.Empty);

        var fullFileContents = await File.ReadAllBytesAsync(testFileName);

        // Upload first chunk
        string firstChunkHash;
        {
            var slice = fullFileContents[..chunkSize];
            firstChunkHash = Convert.ToHexString(SHA256.HashData(slice));
            await using var memStream = new MemoryStream(slice, false);
            var formFile = new FormFile(memStream, 0, slice.Length, "chunk", testFileName);

            var chunkRequest = new UploadChunkRequest(initiateResponse.UploadId, 0, formFile);
            await Service.UploadChunk(chunkRequest);
        }

        var restartResponse = await Service.StartVideoUpload(startVideoUploadRequest);
        
        Assert.That(restartResponse.UploadId, Is.EqualTo(initiateResponse.UploadId));
        Assert.That(restartResponse.UploadedChunks, Has.Exactly(1).Items);
        Assert.That(restartResponse.UploadedChunks[0].Position, Is.EqualTo(0));
        Assert.That(restartResponse.UploadedChunks[0].Sha256Hash, Is.EqualTo(firstChunkHash));

        // Upload second chunk
        {
            var slice = fullFileContents[chunkSize..];
            await using var memStream = new MemoryStream(slice, false);
            var formFile = new FormFile(memStream, 0, slice.Length, "chunk", testFileName);

            var chunkRequest = new UploadChunkRequest(initiateResponse.UploadId, 1, formFile);
            await Service.UploadChunk(chunkRequest);
        }
        
        
        await Service.FinishUpload(new FinishUploadRequest(initiateResponse.UploadId));

        var storagePath = StorageStructureHelper.GetSourcePath(channel.Id, initiateResponse.UploadId);
        await using var resultStream = await ServiceProvider.GetRequiredService<StorageWrapper>()
            .OpenReadStream(storagePath, CancellationToken.None);

        var uploadedHash = await HashStream(resultStream);
        Assert.That(uploadedHash, Is.EqualTo(hash));
    }
    
    [Test]
    public async Task CanUploadChunksOutOfOrder()
    {
        const int chunkSize = 30 * 1024 * 1024;
        var channel = await PrepareTestSystem();
        
        var hash = await GetTestFileHash(testFileName);
        var startVideoUploadRequest = new StartVideoUploadRequest(hash, channel.Id, testFileName,
            new FileInfo(testFileName).Length);
        var initiateResponse = await Service.StartVideoUpload(startVideoUploadRequest);
        
        Assert.That(initiateResponse.UploadedChunks, Is.Empty);

        var fullFileContents = await File.ReadAllBytesAsync(testFileName);

        // Upload second chunk
        {
            var slice = fullFileContents[chunkSize..];
            await using var memStream = new MemoryStream(slice, false);
            var formFile = new FormFile(memStream, 0, slice.Length, "chunk", testFileName);

            var chunkRequest = new UploadChunkRequest(initiateResponse.UploadId, 1, formFile);
            await Service.UploadChunk(chunkRequest);
        }

        // Upload first chunk
        {
            var slice = fullFileContents[..chunkSize];
            await using var memStream = new MemoryStream(slice, false);
            var formFile = new FormFile(memStream, 0, slice.Length, "chunk", testFileName);

            var chunkRequest = new UploadChunkRequest(initiateResponse.UploadId, 0, formFile);
            await Service.UploadChunk(chunkRequest);
        }
        
        await Service.FinishUpload(new FinishUploadRequest(initiateResponse.UploadId));

        var storagePath = StorageStructureHelper.GetSourcePath(channel.Id, initiateResponse.UploadId);
        await using var resultStream = await ServiceProvider.GetRequiredService<StorageWrapper>()
            .OpenReadStream(storagePath, CancellationToken.None);

        var uploadedHash = await HashStream(resultStream);
        Assert.That(uploadedHash, Is.EqualTo(hash));
    }
}