using System.Security.Cryptography;
using System.Text;
using RVM.AuthForge.Domain.Entities;
using RVM.AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RVM.AuthForge.Infrastructure.Services;

public class ApiKeyService(AuthForgeDbContext db) : IApiKeyService
{
    public async Task<(ApplicationApiKey Key, string PlainTextKey)> CreateAsync(string appId, string name)
    {
        var plainKey = GenerateKey();
        var key = new ApplicationApiKey
        {
            AppId = appId,
            Name = name,
            KeyHash = HashKey(plainKey),
            KeyPrefix = plainKey[..8]
        };

        db.ApiKeys.Add(key);
        await db.SaveChangesAsync();
        return (key, plainKey);
    }

    public async Task<ApplicationApiKey?> ValidateAsync(string plainTextKey)
    {
        var hash = HashKey(plainTextKey);
        return await db.ApiKeys
            .FirstOrDefaultAsync(k => k.KeyHash == hash && k.Active);
    }

    public async Task RevokeAsync(Guid id)
    {
        var key = await db.ApiKeys.FindAsync(id);
        if (key is null) return;
        key.Active = false;
        key.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<List<ApplicationApiKey>> ListAsync(string? appId = null)
    {
        var query = db.ApiKeys.AsQueryable();
        if (!string.IsNullOrEmpty(appId))
            query = query.Where(k => k.AppId == appId);
        return await query.OrderByDescending(k => k.CreatedAt).ToListAsync();
    }

    private static string GenerateKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
    }

    private static string HashKey(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexStringLower(hash);
    }
}
