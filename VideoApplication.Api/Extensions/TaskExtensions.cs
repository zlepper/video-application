namespace VideoApplication.Api.Extensions;

internal static class TaskExtensions
{
    public static async Task<T> OnError<T>(this Task<T> task, Func<Task> handleError)
    {
        try
        {
            return await task;
        }
        catch (Exception)
        {
            await handleError();
            throw;
        }
    }

    public static async Task<T> AndThen<T>(this Task<T> t, Func<T, Task> continuation)
    {
        var result = await t;
        await continuation(result);
        return result;
    }
}