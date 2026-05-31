using System.Net;
using LendingSystem.Auth.Application.Auth.GetUserByName;
using LendingSystem.IntegrationTest.Framework.Infrastructure;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Xunit;

namespace LendingSystem.IntegrationTest.Auth;

[WriteTest]
public sealed class GetUserByNameApiTest : IntegrationTestBase 
{
    [Fact]
    public async Task GetUserByName_WithFullEmailAndPassword_ShouldReturnOk()
    {
        // Arrange
        const string username = "searchuser";
        const string email = "getuserbyname@example.com";
        const string password = "password";

        await using var db = CreateDbContext();
        await db.Users.AddRangeAsync(new UserEntity
        {
            UserId = 1000,
            Name = username,
            Email = email,
            PasswordHash = password,
            IsDeleted = false,
        });

        await db.SaveChangesAsync();
        using var client = Factory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/v1/users/{username}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<GetUserByNameResult>(response);
        Assert.True(result.Issuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(username, result.Data.Username);
        Assert.Equal(email, result.Data.Email);
    }
}
