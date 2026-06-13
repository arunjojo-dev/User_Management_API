using Microsoft.EntityFrameworkCore;
using User_Management_API.Middleware;
using User_Management_API.Data;
using User_Management_API.Services;

var builder = WebApplication.CreateBuilder(args);
n// ============================================================================
// STAGE 1: CONFIGURE SERVICES
// ============================================================================
// Add and configure services for dependency injection and API functionality
n// Add ASP.NET Core Controllers
builder.Services.AddControllers();
n// Add OpenAPI/Swagger support for API documentation
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
n// Configure EF Core (SQLite) for simple persistence
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite("Data Source=users.db");
});
n// Register application services
builder.Services.AddScoped<IUserService, UserService>();
n// Add CORS (Cross-Origin Resource Sharing) if needed for frontend integration
// Customize the policy based on your frontend URL
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
n// ============================================================================
// BUILD THE APPLICATION
// ============================================================================
var app = builder.Build();
n// Ensure database is created and seed a couple of users if empty
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        db.Users.AddRange(new[]
        {
            new User_Management_API.Models.User { Name = "John Doe", Email = "john.doe@techhive.com", PhoneNumber = "+1-555-0101", JobTitle = "Senior Developer", Department = "Engineering", CreatedAt = DateTime.UtcNow.AddDays(-30), UpdatedAt = DateTime.UtcNow.AddDays(-1), IsActive = true },
            new User_Management_API.Models.User { Name = "Jane Smith", Email = "jane.smith@techhive.com", PhoneNumber = "+1-555-0102", JobTitle = "Product Manager", Department = "Product", CreatedAt = DateTime.UtcNow.AddDays(-20), UpdatedAt = DateTime.UtcNow.AddDays(-2), IsActive = true }
        });
        db.SaveChanges();
    }
}

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
