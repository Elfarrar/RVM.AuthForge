using RVM.AuthForge.Domain.Entities;
using RVM.AuthForge.Domain.Enums;
using RVM.AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RVM.AuthForge.Infrastructure.Services;

public class AuditLogService(AuthForgeDbContext db) : IAuditLogService
{
    public async Task LogAsync(AuditAction action, string userId, string? userEmail = null,
        string? ipAddress = null, string? userAgent = null, string? details = null)
    {
        db.AuditLog.Add(new AuditLogEntry
        {
            Action = action,
            UserId = userId,
            UserEmail = userEmail,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Details = details
        });
        await db.SaveChangesAsync();
    }

    public async Task<List<AuditLogEntry>> GetEntriesAsync(
        AuditAction? action, string? userId,
        DateTime? from, DateTime? to,
        int page, int pageSize)
    {
        return await BuildQuery(action, userId, from, to)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(AuditAction? action, string? userId,
        DateTime? from, DateTime? to)
    {
        return await BuildQuery(action, userId, from, to).CountAsync();
    }

    private IQueryable<AuditLogEntry> BuildQuery(
        AuditAction? action, string? userId, DateTime? from, DateTime? to)
    {
        var query = db.AuditLog.AsQueryable();
        if (action.HasValue) query = query.Where(a => a.Action == action.Value);
        if (!string.IsNullOrEmpty(userId)) query = query.Where(a => a.UserId == userId);
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);
        return query;
    }
}
