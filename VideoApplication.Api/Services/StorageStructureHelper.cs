namespace VideoApplication.Api.Services;

public static class StorageStructureHelper
{
    public static string GetSourcePath(Guid channelId, Guid videoId)
    {
        return $"channels/{channelId}/videos/{videoId}/source";
    }
    
    public static string GetSourceChunkPath(Guid channelId, Guid videoId, Guid chunkId)
    {
        return $"channels/{channelId}/videos/{videoId}/chunks/{chunkId}";
    }
}