using System.Net;
using System.Net.Http.Json;
using LendingSystem.Auth.Application.Auth;
using LendingSystem.IntegrationTests.Infrastructure;
using Xunit;

namespace LendingSystem.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class GoogleLoginApiTests(IntegrationTestCollectionFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GoogleLogin_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        using var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/google",
            new GoogleLoginCommand("valid-google-token"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<GoogleLoginResult>(response);
        Assert.True(result.Issuccess);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrWhiteSpace(result.Data.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Data.RefreshToken));
    }
}
