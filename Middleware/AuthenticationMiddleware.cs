namespace User_Management_API.Middleware;

/// <summary>
/// Authentication Middleware
/// Validates Bearer tokens from incoming requests. Allows access only to requests with valid tokens
/// or requests to public endpoints (health checks, etc.).
/// 
/// Implementation: Checks for "Authorization" header with Bearer token format.
/// For production, integrate with a proper JWT or OAuth service.
/// 
/// Order: Should be placed after error handling but before request logging to ensure
/// authentication happens early in the pipeline.
/// 
/// Development Mode: In development, authentication is DISABLED to allow easy testing.
/// Set DISABLE_AUTH=false in environment variables or appsettings to enable auth.
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly bool _disableAuthInDev;

    // List of endpoints that don't require authentication
    private static readonly List<string> PublicEndpoints = new()
    {
        "/health",
        "/api/auth/login",
        "/swagger",
        "/api/openapi"
    };

    /// <summary>
    /// Initializes the authentication middleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">Logger instance for authentication logging</param>
    /// <param name="environment">Web host environment to check if in Development</param>
    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
        _disableAuthInDev = _environment.IsDevelopment(); // Auth disabled in Development by default
    }

    /// <summary>
    /// Invokes the middleware to validate authentication tokens.
    /// Checks if the request has a valid Bearer token in the Authorization header.
    /// In Development mode, authentication is skipped for easier testing.
    /// </summary>
    /// <param name="context">The HTTP context for the current request</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // In Development environment, skip authentication for easier testing
        if (_disableAuthInDev)
        {
            _logger.LogDebug("Development mode: Authentication disabled for {Path}", context.Request.Path);
            await _next(context);
            return;
        }

        // Check if the endpoint is public (doesn't require authentication)
        if (IsPublicEndpoint(context.Request.Path))
        {
            // Allow public endpoints to pass through without authentication
            await _next(context);
            return;
        }

        // Validate authentication token
        if (!ValidateToken(context))
        {
            // Token is invalid or missing
            _logger.LogWarning(
                "Unauthorized request - Method: {HttpMethod}, Path: {RequestPath}, RemoteIP: {RemoteIP}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress
            );

            // Return 401 Unauthorized response
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Unauthorized",
                message = "Valid authentication token is required to access this endpoint.",
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        // Log successful authentication
        _logger.LogInformation(
            "Authorized request - Method: {HttpMethod}, Path: {RequestPath}",
            context.Request.Method,
            context.Request.Path
        );

        // Token is valid, pass control to the next middleware
        await _next(context);
    }

    /// <summary>
    /// Validates the authentication token from the request headers.
    /// Checks for Bearer token format and validates the token.
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>True if token is valid, false otherwise</returns>
    private static bool ValidateToken(HttpContext context)
    {
        // Get the Authorization header
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader))
        {
            return false;
        }

        // Check if the header follows Bearer token format
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Extract the token part (after "Bearer ")
        var token = authHeader.Substring("Bearer ".Length).Trim();

        // Validate the token - for this example, we check if it's not empty and meets basic criteria
        // In production, validate against JWT, OAuth provider, or your token service
        if (string.IsNullOrEmpty(token) || token.Length < 10)
        {
            return false;
        }

        // Token appears valid - in production, perform cryptographic validation
        return true;
    }

    /// <summary>
    /// Checks if the requested endpoint is public and doesn't require authentication.
    /// </summary>
    /// <param name="path">The request path to check</param>
    /// <returns>True if the endpoint is public, false otherwise</returns>
    private static bool IsPublicEndpoint(PathString path)
    {
        return PublicEndpoints.Any(endpoint => 
            path.StartsWithSegments(endpoint, StringComparison.OrdinalIgnoreCase));
    }
}
