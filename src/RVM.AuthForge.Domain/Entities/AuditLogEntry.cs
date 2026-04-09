using RVM.AuthForge.Domain.Enums;

namespace RVM.AuthForge.Domain.Entities;

public class AuditLogEntry
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public AuditAction Action { get; set; }
    public required string UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
