using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LendingSystem.Auth.Application.Auth;
using LendingSystem.IntegrationTests.Infrastructure;
using Xunit;

namespace LendingSystem.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class LoginApiTest(IntegrationTestCollectionFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Login_WithEmailAndPassword_ShouldReturnOk()
    {
        const string email = "login.api.test@example.com";
        const string password = "plain-text-password";

        using var client = Factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/users",
            new RegisterUserCommand("loginapitest", email, password));

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/session",
            new LoginCommand(email, password));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        using var body = await JsonDocument.ParseAsync(await loginResponse.Content.ReadAsStreamAsync());
        var root = body.RootElement;
        var data = root.GetProperty("Data");

        Assert.True(root.GetProperty("Issuccess").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("access_token").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("refresh_token").GetString()));
    }
}
