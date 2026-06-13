using Microsoft.EntityFrameworkCore;
using User_Management_API.Data;
using User_Management_API.DTOs;
using User_Management_API.Services;
using Xunit;

namespace User_Management_API.Tests.UnitTests;

public class UserServiceTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateAndRetrieveUser_Succeeds()
    {
        using var db = CreateInMemoryContext();
        var service = new UserService(db);

        var create = new CreateUserDto { Name = "Test User", Email = "test@example.com", JobTitle = "Tester", Department = "QA" };
        var created = await service.CreateAsync(create);

        Assert.NotNull(created);
        Assert.Equal("Test User", created.Name);

        var fetched = await service.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal(created.Email, fetched!.Email);
    }
}
