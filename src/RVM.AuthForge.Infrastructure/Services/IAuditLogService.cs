using RVM.AuthForge.Domain.Entities;
using RVM.AuthForge.Domain.Enums;

namespace RVM.AuthForge.Infrastructure.Services;

public interface IAuditLogService
{
    Task LogAsync(AuditAction action, string userId, string? userEmail = null,
        string? ipAddress = null, string? userAgent = null, string? details = null);

    Task<List<AuditLogEntry>> GetEntriesAsync(
        AuditAction? action = null, string? userId = null,
        DateTime? from = null, DateTime? to = null,
        int page = 1, int pageSize = 50);

    Task<int> CountAsync(AuditAction? action = null, string? userId = null,
        DateTime? from = null, DateTime? to = null);
}
