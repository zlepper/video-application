using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VideoApplication.Api.Extensions;

public static class IndexBuilderExtensions
{
    public static IndexBuilder<T> IncludeAllProperties<T>(this IndexBuilder<T> indexBuilder)
    {
        var indexProperties = indexBuilder.Metadata.Properties;
        var properties = indexBuilder.Metadata.DeclaringEntityType
            .GetProperties()
            .Except(indexProperties)
            .Select(p => p.Name)
            .ToArray();

        return indexBuilder.IncludeProperties(properties);
    }
}