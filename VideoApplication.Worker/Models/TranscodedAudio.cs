namespace VideoApplication.Worker.Models;

public record TranscodedAudio(string Path, string Name) : TranscodedFile(Path);