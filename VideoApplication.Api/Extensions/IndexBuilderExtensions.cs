using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VideoApplication.Api.Extensions;

public static class IndexBuilderExtensions
{
    public static IndexBuilder<T> IncludeAllProperties<T>(this IndexBuilder<T> indexBuilder)
    {
        var alreadyIncluded = indexBuilder.Metadata.Properties.ToList();
        var primaryKey = indexBuilder.Metadata.DeclaringEntityType.FindPrimaryKey();

        if (primaryKey != null)
        {
            alreadyIncluded.AddRange(primaryKey.Properties);
        }
        
        var properties = indexBuilder.Metadata.DeclaringEntityType
            .GetProperties()
            .Except(alreadyIncluded)
            .Select(p => p.Name)
            .ToArray();

        return indexBuilder.IncludeProperties(properties);
    }
}