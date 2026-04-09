using RVM.AuthForge.Domain.Enums;
using RVM.AuthForge.Infrastructure.Services;
using RVM.AuthForge.Tests.Helpers;

namespace RVM.AuthForge.Tests.Services;

public class AuditLogServiceTests
{
    [Fact]
    public async Task LogAsync_CreatesEntry()
    {
        using var db = TestDbContext.Create();
        IAuditLogService service = new AuditLogService(db);

        await service.LogAsync(AuditAction.Login, "user-1", "test@test.com", "127.0.0.1");

        var entries = await service.GetEntriesAsync();
        Assert.Single(entries);
        Assert.Equal(AuditAction.Login, entries[0].Action);
        Assert.Equal("user-1", entries[0].UserId);
    }

    [Fact]
    public async Task GetEntriesAsync_FiltersByAction()
    {
        using var db = TestDbContext.Create();
        IAuditLogService service = new AuditLogService(db);

        await service.LogAsync(AuditAction.Login, "u1");
        await service.LogAsync(AuditAction.Logout, "u2");
        await service.LogAsync(AuditAction.Login, "u3");

        var logins = await service.GetEntriesAsync(action: AuditAction.Login);
        Assert.Equal(2, logins.Count);
        Assert.All(logins, e => Assert.Equal(AuditAction.Login, e.Action));
    }

    [Fact]
    public async Task GetEntriesAsync_FiltersByUserId()
    {
        using var db = TestDbContext.Create();
        IAuditLogService service = new AuditLogService(db);

        await service.LogAsync(AuditAction.Login, "user-A");
        await service.LogAsync(AuditAction.Login, "user-B");

        var entries = await service.GetEntriesAsync(userId: "user-A");
        Assert.Single(entries);
        Assert.Equal("user-A", entries[0].UserId);
    }

    [Fact]
    public async Task GetEntriesAsync_Paginates()
    {
        using var db = TestDbContext.Create();
        IAuditLogService service = new AuditLogService(db);

        for (int i = 0; i < 5; i++)
            await service.LogAsync(AuditAction.Login, $"u{i}");

        var page1 = await service.GetEntriesAsync(page: 1, pageSize: 2);
        var page2 = await service.GetEntriesAsync(page: 2, pageSize: 2);

        Assert.Equal(2, page1.Count);
        Assert.Equal(2, page2.Count);
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        using var db = TestDbContext.Create();
        IAuditLogService service = new AuditLogService(db);

        await service.LogAsync(AuditAction.Login, "u1");
        await service.LogAsync(AuditAction.Logout, "u2");
        await service.LogAsync(AuditAction.Login, "u3");

        Assert.Equal(3, await service.CountAsync());
        Assert.Equal(2, await service.CountAsync(action: AuditAction.Login));
    }

    [Fact]
    public async Task GetEntriesAsync_FiltersByDateRange()
    {
        using var db = TestDbContext.Create();
        IAuditLogService service = new AuditLogService(db);

        await service.LogAsync(AuditAction.Login, "u1");

        var future = DateTime.UtcNow.AddDays(1);
        var entries = await service.GetEntriesAsync(from: future);
        Assert.Empty(entries);
    }
}
