using System.Net;
using System.Net.Http.Json;
using LendingSystem.IntegrationTest.Framework.Infrastructure;
using LendingSystem.Lending.Application.Items;
using LendingSystem.Lending.Application.Items.CreateItem;
using LendingSystem.Lending.Application.Items.GetAllItems;
using LendingSystem.Lending.Application.Items.GetItemByName;
using LendingSystem.Lending.Application.Items.GetItemMedia;
using LendingSystem.Lending.Application.Items.GetItemsByUserName;
using LendingSystem.Lending.Application.Items.UpdateItem;
using LendingSystem.Lending.Application.Items.UploadItemImage;
using LendingSystem.Lending.Application.Items.UploadItemMedia;
using LendingSystem.Lending.Application.Media;
using LendingSystem.Lending.Application.Media.UploadPrivateMedia;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Xunit;

namespace LendingSystem.IntegrationTest.Auth.Items;

[WriteTest]
public sealed class ItemsApiTests : IntegrationTestBase
{
    [Fact]
    public async Task GetAllItems_WithExistingItem_ShouldReturnOk()
    {
        // Arrange
        await SeedUserAndItemAsync();
        using var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/catalog/items");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<IReadOnlyCollection<GetAllItemsResult>>(response);
        Assert.True(result.Issuccess);
        Assert.Contains(result.Data!, item => item.ObjectName == "testitem");
    }

    [Fact]
    public async Task GetItemsByUserName_WithExistingItem_ShouldReturnOk()
    {
        // Arrange
        await SeedUserAndItemAsync();
        using var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/catalog/items/user/owneruser");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<IReadOnlyCollection<GetItemsByUserNameResult>>(response);
        Assert.True(result.Issuccess);
        Assert.Contains(result.Data!, item => item.ObjectName == "testitem");
    }

    [Fact]
    public async Task CreateItem_WithValidBody_ShouldReturnCreated()
    {
        // Arrange
        await SeedUserAsync();
        using var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/v1/catalog/items",
            new CreateItemCommand("createditem", "maker", "cotton", "description"));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await ParseJsonAsync<CreateItemResult>(response);
        Assert.True(result.Issuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("createditem", result.Data.ObjectName);
    }

    [Fact]
    public async Task CreateItemWithForm_WithValidForm_ShouldReturnCreated()
    {
        // Arrange
        await SeedUserAsync();
        using var client = Factory.CreateClient();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("formitem"), "object_name" },
            { new StringContent("maker"), "maker" },
            { new StringContent("cotton"), "material" },
            { new StringContent("description"), "description" }
        };

        // Act
        var response = await client.PostAsync("/api/v1/catalog/items/form", form);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await ParseJsonAsync<CreateItemResult>(response);
        Assert.True(result.Issuccess);
        Assert.Equal("formitem", result.Data!.ObjectName);
    }

    [Fact]
    public async Task GetItemByName_WithExistingItem_ShouldReturnOk()
    {
        // Arrange
        await SeedUserAndItemAsync();
        using var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/catalog/users/owneruser/items/testitem");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<GetItemByNameResult>(response);
        Assert.True(result.Issuccess);
        Assert.Equal("testitem", result.Data!.ObjectName);
    }

    [Fact]
    public async Task UpdateItem_WithValidBody_ShouldReturnOk()
    {
        // Arrange
        await SeedUserAndItemAsync();
        using var client = Factory.CreateClient();

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/v1/catalog/users/owneruser/items/testitem",
            new UpdateItemCommand("updateditem", "newmaker", "linen", "updated", "Available", null));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<UpdateItemResult>(response);
        Assert.True(result.Issuccess);
        Assert.Equal("updateditem", result.Data!.ObjectName);
    }

    [Fact]
    public async Task UploadItemImage_WithImageFile_ShouldReturnOk()
    {
        // Arrange
        await SeedUserAndItemAsync();
        using var client = Factory.CreateClient();
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent([1, 2, 3]), "file", "cover.png");
        form.Last().Headers.ContentType = new("image/png");

        // Act
        var response = await client.PostAsync("/api/v1/catalog/users/owneruser/items/testitem/image", form);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<UploadItemImageResult>(response);
        Assert.True(result.Issuccess);
        Assert.Equal("testitem", result.Data!.ObjectName);
        Assert.Contains("cover.png", result.Data.ImageUrl);
    }

    [Fact]
    public async Task UploadItemMedia_WithImageFile_ShouldReturnCreated()
    {
        // Arrange
        var borrowingKey = await SeedUserItemAndOrderAsync(status: "On Loan");
        using var client = Factory.CreateClient();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("owneruser"), "owner_username" },
            { new StringContent("testitem"), "object_name" },
            { new StringContent(borrowingKey), "borrowing_key" },
            { new StringContent("photo"), "description" },
            { new StringContent("https://example.com"), "link" }
        };
        form.Add(new ByteArrayContent([1, 2, 3]), "file", "media.png");
        form.Last().Headers.ContentType = new("image/png");

        // Act
        var response = await client.PostAsync("/api/v1/catalog/items/media", form);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await ParseJsonAsync<UploadItemMediaResult>(response);
        Assert.True(result.Issuccess);
        Assert.Equal("image", result.Data!.Type);
    }

    [Fact]
    public async Task UploadPrivateMedia_WithImageFile_ShouldReturnCreated()
    {
        // Arrange
        var borrowingKey = await SeedUserItemAndOrderAsync(status: "On Loan");
        using var client = Factory.CreateClient();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("owneruser"), "owner_username" },
            { new StringContent("testitem"), "object_name" },
            { new StringContent(borrowingKey), "borrowing_key" },
            { new StringContent("private photo"), "description" },
            { new StringContent("https://example.com/private"), "link" }
        };
        form.Add(new ByteArrayContent([1, 2, 3]), "file", "private.png");
        form.Last().Headers.ContentType = new("image/png");

        // Act
        var response = await client.PostAsync("/api/v1/media/private", form);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await ParseJsonAsync<UploadPrivateMediaResult>(response);
        Assert.True(result.Issuccess);
        Assert.Equal("image", result.Data!.Type);
    }

    [Fact]
    public async Task GetItemMedia_WithExistingMedia_ShouldReturnOk()
    {
        // Arrange
        await SeedUserItemAndMediaAsync();
        using var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/catalog/users/owneruser/items/testitem/media");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<IReadOnlyCollection<GetItemMediaResult>>(response);
        Assert.True(result.Issuccess);
        Assert.Contains(result.Data!, media => media.Type == "image");
    }

    private async Task SeedUserAsync()
    {
        await using var db = CreateDbContext();
        await db.Users.AddAsync(new UserEntity
        {
            UserId = 1000,
            Name = "owneruser",
            Status = "ACTIVE",
            AuthIdentities =
            [
                new UserAuthIdentityEntity
                {
                    Id = 10000,
                    UserId = 1000,
                    Type = "LOCAL",
                    Identifier = "owner@example.com",
                    MetadataJson = """{"email":"owner@example.com","passwordHash":"password"}"""
                }
            ]
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedUserAndItemAsync()
    {
        await SeedUserAsync();
        await using var db = CreateDbContext();
        await db.Items.AddAsync(new ItemEntity
        {
            ItemId = 2000,
            OwnerId = 1000,
            ObjectName = "testitem",
            Maker = "maker",
            Material = "cotton",
            Description = "description",
            CurrentStatus = "Available"
        });
        await db.SaveChangesAsync();
    }

    private async Task<string> SeedUserItemAndOrderAsync(string status)
    {
        await SeedUserAndItemAsync();
        await using var db = CreateDbContext();
        await db.BorrowerDetails.AddAsync(new BorrowerDetailEntity
        {
            BorrowerDetailId = 3000,
            UserId = 1000,
            BorrowerName = "owneruser",
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        });
        await db.Orders.AddAsync(new OrderEntity
        {
            OrderId = 4000,
            BorrowerDetailId = 3000,
            ObjectId = 2000,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7),
            Status = status
        });
        await db.SaveChangesAsync();
        return PublicResourceKey.FromInt("borrowing", 4000);
    }

    private async Task SeedUserItemAndMediaAsync()
    {
        await SeedUserAndItemAsync();
        await using var db = CreateDbContext();
        await db.ItemMedia.AddAsync(new ItemMediaEntity
        {
            MediaId = 5000,
            ItemId = 2000,
            Type = "image",
            Url = "https://example.com/media.png",
            Description = "media",
            Link = "https://example.com"
        });
        await db.SaveChangesAsync();
    }
}
