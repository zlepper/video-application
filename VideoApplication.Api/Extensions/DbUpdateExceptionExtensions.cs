using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace VideoApplication.Api.Extensions;

public static class DbUpdateExceptionExtensions
{
    public static bool IsUniqueConstraintViolation(this DbUpdateException e)
    {
        return e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}