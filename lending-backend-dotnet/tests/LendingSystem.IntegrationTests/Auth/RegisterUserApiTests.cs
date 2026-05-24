using System.Text.Json;
using System.Net;
using System.Net.Http.Json;
using LendingSystem.Auth.Application.Auth;
using LendingSystem.IntegrationTests.Infrastructure;
using LendingSystem.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LendingSystem.IntegrationTests.Auth;

public sealed class RegisterUserApiTests(
    DatabaseSchemaFixture database,
    LendingWebApplicationFactory factory) : IClassFixture<DatabaseSchemaFixture>, IClassFixture<LendingWebApplicationFactory>
{
    private readonly DatabaseSchemaFixture _database = database;
    private readonly LendingWebApplicationFactory _factory = factory;

    [Fact]
    public async Task Register_creates_user_through_api()
    {
        const string email = "register.api.test@example.com";
        const string password = "plain-text-password";

        await using (var db = IntegrationTestDatabase.CreateDbContext())
        {
            await db.Users
                .Where(x => x.Email == email)
                .ExecuteDeleteAsync();
        }

        using var client = _factory.CreateClient();

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

        await using var verifyDb = IntegrationTestDatabase.CreateDbContext();
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

        using var client = _factory.CreateClient();

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
        
        await using var verifyDb = IntegrationTestDatabase.CreateDbContext();
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
