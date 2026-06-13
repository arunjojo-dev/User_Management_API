using Microsoft.EntityFrameworkCore;
using User_Management_API.Data;
using User_Management_API.DTOs;
using User_Management_API.Models;

namespace User_Management_API.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;

    public UserService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        return await _db.Users
            .Where(u => u.IsActive)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                JobTitle = u.JobTitle,
                Department = u.Department,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                IsActive = u.IsActive
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (u == null) return null;
        return new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            JobTitle = u.JobTitle,
            Department = u.Department,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt,
            IsActive = u.IsActive
        };
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        // check duplicate email
        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.IsActive))
            throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            JobTitle = dto.JobTitle,
            Department = dto.Department,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            JobTitle = user.JobTitle,
            Department = user.Department,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsActive = user.IsActive
        };
    }

    public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        if (existing == null) return null;

        // duplicate email check
        if (!existing.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase) &&
            await _db.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.IsActive))
        {
            throw new InvalidOperationException("Email already exists");
        }

        existing.Name = dto.Name;
        existing.Email = dto.Email;
        existing.PhoneNumber = dto.PhoneNumber;
        existing.JobTitle = dto.JobTitle;
        existing.Department = dto.Department;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new UserDto
        {
            Id = existing.Id,
            Name = existing.Name,
            Email = existing.Email,
            PhoneNumber = existing.PhoneNumber,
            JobTitle = existing.JobTitle,
            Department = existing.Department,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = existing.UpdatedAt,
            IsActive = existing.IsActive
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        if (existing == null) return false;
        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
