using System.ComponentModel.DataAnnotations;

namespace User_Management_API.DTOs;

public class CreateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [Required]
    [StringLength(50)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Department { get; set; } = string.Empty;
}
