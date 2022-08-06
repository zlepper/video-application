namespace VideoApplication.Shared.Setup;

public class RouteName
{
    public static readonly RouteName Api = new("api");
    public static readonly RouteName Worker = new("worker");

    private readonly string Value;

    private RouteName(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(RouteName r) => r.Value;
}