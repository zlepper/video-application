namespace VideoApplication.Worker.Models;

public record TranscodedVideo(string Path, int Height, bool VideoOnly) : TranscodedFile(Path);