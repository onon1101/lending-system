using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LendingSystem.IntegrationTests.Infrastructure;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Xunit;

namespace LendingSystem.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class SearchUserByNameApiTest(IntegrationTestCollectionFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SearchUser_WithIncompleteWordsInUsername_ShouldReturnOk()
    {
        // Arrange
        await using var db = IntegrationTestDatabase.CreateDbContext();
        await db.Users.AddRangeAsync(new UserEntity
        {
            UserId = 1000,
            Name = "searchuser",
            Email = "search-user-email",
            PasswordHash = "",
            IsDeleted = false,
        });

        await db.SaveChangesAsync();
        using var client = Factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/v1/users/search/sear");
        
        // Asserts
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = body.RootElement;
        var data = root.GetProperty("Data");
        
        Assert.True(root.GetProperty("Issuccess").GetBoolean());
        Assert.Equal("searchuser", data.GetProperty("name").GetString());
        Assert.Equal("search-user-email", data.GetProperty("email").GetString());
    }
}
