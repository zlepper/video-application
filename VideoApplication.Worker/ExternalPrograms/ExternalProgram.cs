namespace VideoApplication.Worker.ExternalPrograms;

public class ExternalProgram
{
    public readonly string ProgramName;

    public ExternalProgram(string programName)
    {
        ProgramName = programName;
    }
    
    public static ExternalProgram Ffprobe => new("ffprobe");
}