using User_Management_API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// STAGE 1: CONFIGURE SERVICES
// ============================================================================
// Add and configure services for dependency injection and API functionality

// Add ASP.NET Core Controllers
builder.Services.AddControllers();

// Add OpenAPI/Swagger support for API documentation
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS (Cross-Origin Resource Sharing) if needed for frontend integration
// Customize the policy based on your frontend URL
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// ============================================================================
// BUILD THE APPLICATION
// ============================================================================
var app = builder.Build();

// ============================================================================
// STAGE 2: CONFIGURE MIDDLEWARE PIPELINE
// ============================================================================
// Middleware runs in the order it is registered.
// IMPORTANT: Order matters! Error handling should be first to catch all exceptions.

// 1. ERROR HANDLING MIDDLEWARE (FIRST - catches all exceptions)
// Wraps all downstream middleware and returns standardized error responses
app.UseMiddleware<ErrorHandlingMiddleware>();

// 2. AUTHENTICATION MIDDLEWARE (SECOND - validates tokens early)
// Ensures only authenticated requests proceed to business logic
app.UseMiddleware<AuthenticationMiddleware>();

// 3. LOGGING MIDDLEWARE (THIRD - logs actual processing time)
// Logs request/response details after authentication for accurate metrics
app.UseMiddleware<RequestLoggingMiddleware>();

// Configure OpenAPI documentation endpoint
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// HTTPS Redirection - enforce secure connections
// Routes HTTP requests to HTTPS
app.UseHttpsRedirection();

// ============================================================================
// STAGE 3: CONFIGURE ENDPOINTS AND MAP CONTROLLERS
// ============================================================================

// Use CORS policy configured above
app.UseCors("AllowAll");

// Route requests to controller endpoints
app.MapControllers();

// ============================================================================
// START THE APPLICATION
// ============================================================================
// Starts listening for HTTP requests on the configured ports
// HTTP:  http://localhost:5090
// HTTPS: https://localhost:7243
app.Run();
