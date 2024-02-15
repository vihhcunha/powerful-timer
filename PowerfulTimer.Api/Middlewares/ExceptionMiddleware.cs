namespace PowerfulTimer.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<string> logger)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleRequestExceptionAsync(ex, logger, context);
        }
    }

    private static async Task HandleRequestExceptionAsync(Exception exception, ILogger<string> logger, HttpContext context)
    {
        if (context.Response.HasStarted)
            return;
        
        logger.LogError(exception, "Unhandled exception: " + exception.Message);
        context.Response.StatusCode = 500;
        await context.Response.CompleteAsync();
    }
}
