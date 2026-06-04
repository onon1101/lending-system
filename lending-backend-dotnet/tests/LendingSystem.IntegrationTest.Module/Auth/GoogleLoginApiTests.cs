using System.Net;
using System.Net.Http.Json;
using LendingSystem.Auth.Application.Auth.GoogleLogin;
using LendingSystem.IntegrationTest.Framework.Infrastructure;
using Xunit;

namespace LendingSystem.IntegrationTest.Auth.Auth;

[WriteTest]
public sealed class GoogleLoginApiTests : IntegrationTestBase
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
