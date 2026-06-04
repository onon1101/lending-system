using System.Net;
using System.Text.Json;
using LendingSystem.IntegrationTest.Framework.Infrastructure;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Xunit;

namespace LendingSystem.IntegrationTest.Auth.Auth;

[WriteTest]
public sealed class SearchUserByNameApiTest : IntegrationTestBase
{
    [Fact]
    public async Task SearchUser_WithIncompleteWordsInUsername_ShouldReturnOk()
    {
        // Arrange
        await using var db = CreateDbContext();
        await db.Users.AddRangeAsync(new UserEntity
        {
            UserId = 1000,
            Name = "searchuser",
            Status = "ACTIVE",
            AuthIdentities =
            [
                new UserAuthIdentityEntity
                {
                    Id = 10000,
                    UserId = 1000,
                    Type = "LOCAL",
                    Identifier = "search-user-email",
                    MetadataJson = """{"email":"search-user-email","passwordHash":""}"""
                }
            ]
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
