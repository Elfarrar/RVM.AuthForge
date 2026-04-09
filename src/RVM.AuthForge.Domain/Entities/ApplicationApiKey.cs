namespace RVM.AuthForge.Domain.Entities;

public class ApplicationApiKey
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string AppId { get; set; }
    public required string Name { get; set; }
    public required string KeyHash { get; set; }
    public required string KeyPrefix { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
}
