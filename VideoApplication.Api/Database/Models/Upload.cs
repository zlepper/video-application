using NodaTime;

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
    
    public Instant UploadStartDate { get; set; }
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

public class Video
{
    public Guid Id { get; set; }
    
    public Guid ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;
    
    public string OriginalFileName { get; set; } = null!;

    public Instant UploadDate { get; set; }
    
    public Instant? PublishDate { get; set; }

    public string Name { get; set; } = null!;
    
    public VideoProcessingState ProcessingState { get; set; }
    
    public TimeSpan Duration { get; set; }
    
    public TimeSpan ProcessedDuration { get; set; } 

    public ICollection<VideoVideoTrack> VideoTracks { get; set; } = null!;
    public ICollection<VideoAudioTrack> AudioTracks { get; set; } = null!;
}

public enum VideoProcessingState
{
    Processing,
    Ready
}

public class VideoVideoTrack
{
    public Guid Id { get; set; }
    
    public Guid VideoId { get; set; }
    public Video Video { get; set; } = null!;
    
    public int Height { get; set; }
    public int FrameRate { get; set; }
}

public class VideoAudioTrack
{
    public Guid Id { get; set; }
    
    public Guid VideoId { get; set; }
    public Video Video { get; set; } = null!;

    public string Name { get; set; } = null!;
    
    public int Index { get; set; }
}