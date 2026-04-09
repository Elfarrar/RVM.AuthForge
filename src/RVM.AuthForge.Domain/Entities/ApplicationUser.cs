using Microsoft.AspNetCore.Identity;

namespace RVM.AuthForge.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string FullName { get; set; }
    public bool Active { get; set; } = true;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
