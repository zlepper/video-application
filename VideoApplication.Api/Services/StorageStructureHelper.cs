namespace VideoApplication.Api.Services;

public static class StorageStructureHelper
{
    public static string GetSourcePath(Guid channelId, Guid videoId)
    {
        return $"channels/{channelId}/videos/{videoId}/source";
    }
}