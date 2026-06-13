using System.Diagnostics;

namespace User_Management_API.Middleware;

/// <summary>
/// Request/Response Logging Middleware
/// Logs details of incoming HTTP requests and outgoing responses for auditing and debugging purposes.
/// Captures: HTTP method, request path, response status code, and request duration.
/// 
/// Order: Should be placed after error handling and authentication middleware to capture
/// the actual processing time including downstream middleware.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes the request logging middleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">Logger instance for request/response logging</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to log request and response information.
    /// Measures the time taken to process the request and logs the response status.
    /// </summary>
    /// <param name="context">The HTTP context for the current request</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // Start stopwatch to measure request processing duration
        var stopwatch = Stopwatch.StartNew();

        // Store the original response body stream
        var originalBodyStream = context.Response.Body;

        // Create a memory stream to capture the response body for logging
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            // Log incoming request information
            LogRequest(context);

            try
            {
                // Pass control to the next middleware in the pipeline
                await _next(context);
            }
            finally
            {
                // Stop the stopwatch and calculate elapsed time
                stopwatch.Stop();

                // Log outgoing response information including status code and duration
                LogResponse(context, stopwatch.ElapsedMilliseconds);

                // Rewind the response body stream so it can be read and copied back
                responseBody.Seek(0, System.IO.SeekOrigin.Begin);

                // Copy the captured response body back to the original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }

    /// <summary>
    /// Logs details of the incoming HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context containing request information</param>
    private void LogRequest(HttpContext context)
    {
        var request = context.Request;
        
        _logger.LogInformation(
            "Incoming Request - Method: {HttpMethod}, Path: {RequestPath}, " +
            "Query: {QueryString}, ContentType: {ContentType}, RemoteIP: {RemoteIP}",
            request.Method,
            request.Path,
            request.QueryString.HasValue ? request.QueryString.Value : "None",
            request.ContentType ?? "None",
            context.Connection.RemoteIpAddress
        );
    }

    /// <summary>
    /// Logs details of the outgoing HTTP response.
    /// </summary>
    /// <param name="context">The HTTP context containing response information</param>
    /// <param name="elapsedMilliseconds">Time taken to process the request in milliseconds</param>
    private void LogResponse(HttpContext context, long elapsedMilliseconds)
    {
        var response = context.Response;
        
        // Determine log level based on HTTP status code
        var logLevel = response.StatusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };

        _logger.Log(
            logLevel,
            "Outgoing Response - Method: {HttpMethod}, Path: {RequestPath}, " +
            "StatusCode: {StatusCode}, Duration: {Duration}ms",
            context.Request.Method,
            context.Request.Path,
            response.StatusCode,
            elapsedMilliseconds
        );
    }
}
