namespace VideoApplication.Api.Database.Models;

public class Upload
{
    public Guid Id { get; set; }
    
    public Guid ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;

    public string Sha256Hash { get; set; } = null!;
    public long FileSize { get; set; }

    public string FileName { get; set; } = null!;

    public ICollection<UploadChunk> Chunks { get; set; } = null!;

    public string StorageUploadId { get; set; } = null!;
}

public class UploadChunk
{
    public Guid Id { get; set; }
    
    public Guid UploadId { get; set; }
    public Upload Upload { get; set; } = null!;
    
    public int Position { get; set; }

    public string? Sha256Hash { get; set; }
    
    public string? StorageETag { get; set; }
}