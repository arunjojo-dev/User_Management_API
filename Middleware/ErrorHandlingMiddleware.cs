namespace User_Management_API.Middleware;

/// <summary>
/// Error Handling Middleware
/// Catches all unhandled exceptions in the request pipeline and returns
/// standardized JSON error responses to maintain API consistency.
/// 
/// Order: Should be first in the middleware pipeline to catch all exceptions.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes the error handling middleware with the next middleware in the pipeline.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">Logger instance for error logging</param>
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware. Wraps the next middleware in a try-catch block
    /// to intercept and handle any exceptions that occur during request processing.
    /// </summary>
    /// <param name="context">The HTTP context for the current request</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Pass control to the next middleware in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the exception with full details for debugging
            _logger.LogError(ex, "An unhandled exception occurred in the request pipeline. " +
                $"Path: {context.Request.Path}, Method: {context.Request.Method}");

            // Handle the exception and return error response
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles exceptions by setting appropriate HTTP status codes and returning
    /// standardized JSON error responses.
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="exception">The exception that was thrown</param>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Set response content type to JSON
        context.Response.ContentType = "application/json";

        // Create error response object
        var response = new
        {
            error = "An error occurred while processing your request.",
            details = exception.Message,
            timestamp = DateTime.UtcNow
        };

        // Set HTTP status code based on exception type
        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        // Write error response to response body
        return context.Response.WriteAsJsonAsync(response);
    }
}
