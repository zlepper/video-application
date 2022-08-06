namespace VideoApplication.Shared.Storage;

public static class StorageStructureHelper
{
    public static string GetSourcePath(Guid channelId, Guid videoId)
    {
        return $"channels/{channelId}/videos/{videoId}/source";
    }

    public static string GetVideoStreamPath(Guid channelId, Guid videoId, string relativePath)
    {
        return $"channels/{channelId}/videos/{videoId}/streams/{relativePath.Replace('\\', '/')}";
    }
}