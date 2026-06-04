using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LendingSystem.Auth.Application.Auth.RegisterUser;
using LendingSystem.IntegrationTest.Framework.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LendingSystem.IntegrationTest.Auth.Auth;

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
        var identity = await verifyDb.UserAuthIdentities
            .Include(x => x.User)
            .SingleAsync(x => x.Type == "LOCAL" && x.Identifier == email);
        var user = identity.User!;

        Assert.True(user.UserId > int.MaxValue);
        Assert.Equal("registerapitest", user.Name);
        using var metadata = JsonDocument.Parse(identity.MetadataJson);
        var passwordHash = metadata.RootElement.GetProperty("passwordHash").GetString();
        Assert.NotEqual(password, passwordHash);
        Assert.StartsWith("$2", passwordHash);
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
        var identity = await verifyDb.UserAuthIdentities
            .Include(x => x.User)
            .SingleAsync(x => x.Type == "LOCAL" && x.Identifier == email);
        var user = identity.User!;

        Assert.True(user.UserId > int.MaxValue);
        Assert.Equal(username, user.Name);
        using var metadata = JsonDocument.Parse(identity.MetadataJson);
        var passwordHash = metadata.RootElement.GetProperty("passwordHash").GetString();
        Assert.NotEqual(password, passwordHash);
        Assert.StartsWith("$2", passwordHash);
        
        var existResponse = await client.PostAsJsonAsync(
            "/api/v1/users",
            new RegisterUserCommand(username, email, password));
        
        Assert.Equal(HttpStatusCode.BadRequest, existResponse.StatusCode);
        using var existBody = await JsonDocument.ParseAsync(await existResponse.Content.ReadAsStreamAsync());
        var existRoot = existBody.RootElement;
        Assert.False(existRoot.GetProperty("Issuccess").GetBoolean());
    }
}
