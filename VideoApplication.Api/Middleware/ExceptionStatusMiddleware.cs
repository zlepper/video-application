using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace VideoApplication.Api.Middleware;

public class ExceptionStatusMiddleware
{
    private readonly ILogger<ExceptionStatusMiddleware> _logger;

    public ExceptionStatusMiddleware(ILogger<ExceptionStatusMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (BaseStatusException statusException)
        {
            var error = statusException.GetError();
            _logger.LogInformation(statusException, "Request failed: {Error}", error);
            await ReturnError(context, error);
        }
        catch (Exception e)
        {
            var error = new UnknownInternalErrorResponse()
            {
                Error = ErrorKind.InternalServerError,
                Message = $"Internal error: {e.Message}"
            };
            _logger.LogError(e, "Internal server error: {Exception}", e);
            await ReturnError(context, error);
        }
    }

    private record UnknownInternalErrorResponse : ErrorResponse;
    
    private async Task ReturnError(HttpContext context, ErrorResponse error)
    {
        if (context.Response.HasStarted)
        {
            // Something went wrong, but we can't do anything to stop it now.
            // Assume dotnet itself will be able to handle it
            _logger.LogInformation("Request has already started, leaving handling to dotnet");
            return;
        }


        if (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug("Request has been aborted, stopping error processing");
            // We don't care then
            return;
        }

        context.Response.StatusCode = (int) error.Error;

        var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());
        var resultExecutor = context.RequestServices.GetRequiredService<IActionResultExecutor<JsonResult>>();
        await resultExecutor.ExecuteAsync(actionContext, new JsonResult(error));
    }
}