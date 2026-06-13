using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using User_Management_API.Models;

namespace User_Management_API.Controllers;

/// <summary>
/// User Management API Controller
/// Provides CRUD (Create, Read, Update, Delete) operations for managing user records.
/// All endpoints require Bearer token authentication (except this documentation).
/// 
/// Base URL: /api/users
/// 
/// This controller handles:
/// - Creating new user records
/// - Retrieving all users or specific users by ID
/// - Updating existing user information
/// - Deleting user records
/// - Input validation and error handling
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// In-memory user storage for this demo. In production, replace with database context.
    /// </summary>
    private static List<User> _users = new()
    {
        new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john.doe@techhive.com",
            PhoneNumber = "+1-555-0101",
            JobTitle = "Senior Developer",
            Department = "Engineering",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            IsActive = true
        },
        new User
        {
            Id = 2,
            Name = "Jane Smith",
            Email = "jane.smith@techhive.com",
            PhoneNumber = "+1-555-0102",
            JobTitle = "Product Manager",
            Department = "Product",
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            IsActive = true
        }
    };

    private static int _nextId = 3;

    public UsersController(ILogger<UsersController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all active users.
    /// </summary>
    /// <returns>
    /// 200 OK: List of all active users
    /// 500 Internal Server Error: If an unexpected error occurs
    /// </returns>
    /// <example>
    /// GET /api/users
    /// Authorization: Bearer {valid_token}
    /// </example>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetAllUsers()
    {
        try
        {
            _logger.LogInformation("Retrieving all active users. Total count: {Count}", _users.Count(u => u.IsActive));

            // Filter only active users
            var activeUsers = _users.Where(u => u.IsActive).ToList();

            return Ok(activeUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all users");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a specific user by their unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user to retrieve</param>
    /// <returns>
    /// 200 OK: The requested user object
    /// 404 Not Found: If user with the specified ID doesn't exist or is inactive
    /// 500 Internal Server Error: If an unexpected error occurs
    /// </returns>
    /// <example>
    /// GET /api/users/1
    /// Authorization: Bearer {valid_token}
    /// </example>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetUserById(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving user with ID: {UserId}", id);

            // Find user by ID and check if active
            var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found or is inactive", id);

                return NotFound(new
                {
                    error = "User not found",
                    message = $"No active user found with ID {id}",
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user with ID {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Creates a new user record with validation.
    /// Validates that all required fields are provided and properly formatted.
    /// </summary>
    /// <param name="user">The user object containing user details to be created</param>
    /// <returns>
    /// 201 Created: The newly created user object with generated ID
    /// 400 Bad Request: If validation fails or required fields are missing
    /// 500 Internal Server Error: If an unexpected error occurs
    /// </returns>
    /// <example>
    /// POST /api/users
    /// Authorization: Bearer {valid_token}
    /// Content-Type: application/json
    /// 
    /// {
    ///   "name": "Alice Johnson",
    ///   "email": "alice.johnson@techhive.com",
    ///   "phoneNumber": "+1-555-0103",
    ///   "jobTitle": "QA Engineer",
    ///   "department": "Quality Assurance"
    /// }
    /// </example>
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CreateUser([FromBody] User user)
    {
        try
        {
            // Validate user object
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("User creation failed due to validation errors. Errors: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));

                return BadRequest(new
                {
                    error = "Validation failed",
                    details = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)),
                    timestamp = DateTime.UtcNow
                });
            }

            // Check for duplicate email
            if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase) && u.IsActive))
            {
                _logger.LogWarning("User creation failed - email already exists: {Email}", user.Email);

                return BadRequest(new
                {
                    error = "Email already exists",
                    message = $"A user with email '{user.Email}' already exists",
                    timestamp = DateTime.UtcNow
                });
            }

            // Assign new ID and timestamps
            user.Id = _nextId++;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsActive = true;

            // Add user to storage
            _users.Add(user);

            _logger.LogInformation("New user created successfully. ID: {UserId}, Name: {UserName}, Email: {Email}", 
                user.Id, user.Name, user.Email);

            // Return 201 Created with the created user object
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing user's information.
    /// Validates all fields and ensures no duplicate emails are created.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update</param>
    /// <param name="user">The updated user object with new values</param>
    /// <returns>
    /// 200 OK: The updated user object
    /// 400 Bad Request: If validation fails or email is already in use
    /// 404 Not Found: If user with the specified ID doesn't exist
    /// 500 Internal Server Error: If an unexpected error occurs
    /// </returns>
    /// <example>
    /// PUT /api/users/1
    /// Authorization: Bearer {valid_token}
    /// Content-Type: application/json
    /// 
    /// {
    ///   "name": "John Updated",
    ///   "email": "john.updated@techhive.com",
    ///   "phoneNumber": "+1-555-0199",
    ///   "jobTitle": "Lead Developer",
    ///   "department": "Engineering"
    /// }
    /// </example>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateUser(int id, [FromBody] User user)
    {
        try
        {
            // Validate the input
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("User update failed for ID {UserId} - validation errors", id);

                return BadRequest(new
                {
                    error = "Validation failed",
                    details = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)),
                    timestamp = DateTime.UtcNow
                });
            }

            // Find the user to update
            var existingUser = _users.FirstOrDefault(u => u.Id == id);

            if (existingUser == null || !existingUser.IsActive)
            {
                _logger.LogWarning("User update failed - user not found. ID: {UserId}", id);

                return NotFound(new
                {
                    error = "User not found",
                    message = $"No active user found with ID {id}",
                    timestamp = DateTime.UtcNow
                });
            }

            // Check for duplicate email (excluding the current user)
            if (!existingUser.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase) &&
                _users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase) && u.IsActive))
            {
                _logger.LogWarning("User update failed for ID {UserId} - email already exists: {Email}", id, user.Email);

                return BadRequest(new
                {
                    error = "Email already exists",
                    message = $"A user with email '{user.Email}' already exists",
                    timestamp = DateTime.UtcNow
                });
            }

            // Update user properties
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.JobTitle = user.JobTitle;
            existingUser.Department = user.Department;
            existingUser.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("User updated successfully. ID: {UserId}, Name: {UserName}", id, existingUser.Name);

            return Ok(existingUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user with ID {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Deletes (soft delete) a user record by marking it as inactive.
    /// Uses soft delete approach to maintain data integrity for auditing purposes.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete</param>
    /// <returns>
    /// 204 No Content: Successfully deleted (no response body)
    /// 404 Not Found: If user with the specified ID doesn't exist
    /// 500 Internal Server Error: If an unexpected error occurs
    /// </returns>
    /// <example>
    /// DELETE /api/users/1
    /// Authorization: Bearer {valid_token}
    /// </example>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult DeleteUser(int id)
    {
        try
        {
            // Find the user to delete
            var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("User deletion failed - user not found. ID: {UserId}", id);

                return NotFound(new
                {
                    error = "User not found",
                    message = $"No active user found with ID {id}",
                    timestamp = DateTime.UtcNow
                });
            }

            // Soft delete: mark user as inactive instead of removing from database
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("User deleted successfully (soft delete). ID: {UserId}, Name: {UserName}", id, user.Name);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user with ID {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Health check endpoint to verify API is running.
    /// This endpoint is public and doesn't require authentication.
    /// </summary>
    /// <returns>200 OK with API status</returns>
    [HttpGet("/health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "User Management API",
            timestamp = DateTime.UtcNow
        });
    }
}
