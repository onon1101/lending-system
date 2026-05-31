using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LendingSystem.Auth.Application.Auth;
using LendingSystem.IntegrationTest.Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LendingSystem.IntegrationTest.Auth;

[WriteTest]
public sealed class RegisterUserApiTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_creates_user_through_api()
    {
        const string email = "register.api.test@example.com";
        const string password = "plain-text-password";

        using var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/users",
            new RegisterUserCommand("registerapitest", email, password));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = body.RootElement;
        var data = root.GetProperty("Data");

        Assert.True(root.GetProperty("Issuccess").GetBoolean());
        Assert.Equal("registerapitest", data.GetProperty("name").GetString());
        Assert.Equal(email, data.GetProperty("email").GetString());

        await using var verifyDb = CreateDbContext();
        var user = await verifyDb.Users.SingleAsync(x => x.Email == email);

        Assert.True(user.UserId > int.MaxValue);
        Assert.Equal("registerapitest", user.Name);
        Assert.NotEqual(password, user.PasswordHash);
        Assert.StartsWith("$2", user.PasswordHash);
    }

    [Fact]
    public async Task Register_already_exist_user_api()
    {
        const string username = "registerexistapitest";
        const string email = "register.exist.api.test@example.com";
        const string password = "plain-text-password";

        using var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/users",
            new RegisterUserCommand(username, email, password));
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = body.RootElement;
        var data = root.GetProperty("Data");
        
        Assert.True(root.GetProperty("Issuccess").GetBoolean());
        Assert.Equal(username, data.GetProperty("name").GetString());
        Assert.Equal(email, data.GetProperty("email").GetString());
        
        await using var verifyDb = CreateDbContext();
        var user = await verifyDb.Users.SingleAsync(x => x.Email == email);

        Assert.True(user.UserId > int.MaxValue);
        Assert.Equal(username, user.Name);
        Assert.NotEqual(password, user.PasswordHash);
        Assert.StartsWith("$2", user.PasswordHash);
        
        var existResponse = await client.PostAsJsonAsync(
            "/api/v1/users",
            new RegisterUserCommand(username, email, password));
        
        Assert.Equal(HttpStatusCode.BadRequest, existResponse.StatusCode);
        using var existBody = await JsonDocument.ParseAsync(await existResponse.Content.ReadAsStreamAsync());
        var existRoot = existBody.RootElement;
        Assert.False(existRoot.GetProperty("Issuccess").GetBoolean());
    }
}
