namespace VideoApplication.Api.Extensions;

public static class TypeExtensions
{
    public static string GetPrettyTypeName(this Type type)
    {
        if (type.IsGenericType)
        {
            var rootTypeName =
                type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.InvariantCultureIgnoreCase));
            if (type.IsConstructedGenericType)
            {
                var arguments = type.GenericTypeArguments;
                return $"{rootTypeName}<{string.Join(", ", arguments.Select(GetPrettyTypeName))}>";
            }
            else
            {
                var arguments = type.GetGenericArguments();
                return $"{rootTypeName}<{string.Join(",", new string[arguments.Length])}>";
            }
        }

        return type.Name;
    }

}