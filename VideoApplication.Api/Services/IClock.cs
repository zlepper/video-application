namespace VideoApplication.Api.Services;

public interface IClock
{
    public DateTimeOffset Now { get; } 
}

public class DefaultClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}