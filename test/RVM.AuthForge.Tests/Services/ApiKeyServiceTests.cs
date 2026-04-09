using RVM.AuthForge.Infrastructure.Services;
using RVM.AuthForge.Tests.Helpers;

namespace RVM.AuthForge.Tests.Services;

public class ApiKeyServiceTests
{
    [Fact]
    public async Task CreateAsync_ReturnsKeyWithPrefix()
    {
        using var db = TestDbContext.Create();
        var service = new ApiKeyService(db);

        var (key, plainKey) = await service.CreateAsync("my-app", "Test Key");

        Assert.Equal("my-app", key.AppId);
        Assert.Equal("Test Key", key.Name);
        Assert.Equal(8, key.KeyPrefix.Length);
        Assert.StartsWith(key.KeyPrefix, plainKey);
        Assert.True(key.Active);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsKeyForValidPlainText()
    {
        using var db = TestDbContext.Create();
        var service = new ApiKeyService(db);

        var (_, plainKey) = await service.CreateAsync("app1", "Key1");
        var validated = await service.ValidateAsync(plainKey);

        Assert.NotNull(validated);
        Assert.Equal("app1", validated.AppId);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsNullForInvalidKey()
    {
        using var db = TestDbContext.Create();
        var service = new ApiKeyService(db);

        var result = await service.ValidateAsync("invalid-key-here");
        Assert.Null(result);
    }

    [Fact]
    public async Task RevokeAsync_DeactivatesKey()
    {
        using var db = TestDbContext.Create();
        var service = new ApiKeyService(db);

        var (key, plainKey) = await service.CreateAsync("app1", "Key1");
        await service.RevokeAsync(key.Id);

        var validated = await service.ValidateAsync(plainKey);
        Assert.Null(validated);
    }

    [Fact]
    public async Task ListAsync_ReturnsAllKeys()
    {
        using var db = TestDbContext.Create();
        var service = new ApiKeyService(db);

        await service.CreateAsync("app-a", "Key1");
        await service.CreateAsync("app-b", "Key2");
        await service.CreateAsync("app-a", "Key3");

        var all = await service.ListAsync();
        Assert.Equal(3, all.Count);

        var filtered = await service.ListAsync("app-a");
        Assert.Equal(2, filtered.Count);
    }

    [Fact]
    public async Task CreateAsync_HashesKeyWithSHA256()
    {
        using var db = TestDbContext.Create();
        var service = new ApiKeyService(db);

        var (key, _) = await service.CreateAsync("app", "Key");

        Assert.Equal(64, key.KeyHash.Length); // SHA256 hex = 64 chars
        Assert.Matches("^[0-9a-f]+$", key.KeyHash);
    }

    [Fact]
    public async Task RevokeAsync_SetsRevokedAt()
    {
        using var db = TestDbContext.Create();
        var service = new ApiKeyService(db);

        var (key, _) = await service.CreateAsync("app", "Key");
        Assert.Null(key.RevokedAt);

        await service.RevokeAsync(key.Id);

        var keys = await service.ListAsync();
        var revoked = keys.First(k => k.Id == key.Id);
        Assert.NotNull(revoked.RevokedAt);
        Assert.False(revoked.Active);
    }
}
