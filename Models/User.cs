using System.ComponentModel.DataAnnotations;

namespace User_Management_API.Models;

/// <summary>
/// Represents a User entity in the User Management API.
/// Contains all essential user information with validation rules.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user. Auto-generated on creation.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Full name of the user. Required field, must be between 2 and 100 characters.
    /// </summary>
    [Required(ErrorMessage = "User name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the user. Required and must be a valid email format.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number of the user. Optional, but must follow valid phone format if provided.
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Job title or position of the user within the organization.
    /// Required field, max 50 characters.
    /// </summary>
    [Required(ErrorMessage = "Job title is required.")]
    [StringLength(50, ErrorMessage = "Job title cannot exceed 50 characters.")]
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Department where the user works. Required field.
    /// </summary>
    [Required(ErrorMessage = "Department is required.")]
    [StringLength(50, ErrorMessage = "Department cannot exceed 50 characters.")]
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the user record was created. Set automatically.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of the last user record update. Updated automatically on modifications.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the user account is active. Soft delete approach.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
