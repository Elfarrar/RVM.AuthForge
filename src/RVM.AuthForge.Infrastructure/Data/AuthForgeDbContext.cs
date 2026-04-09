using RVM.AuthForge.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RVM.AuthForge.Infrastructure.Data;

public class AuthForgeDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public DbSet<ApplicationApiKey> ApiKeys => Set<ApplicationApiKey>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    public AuthForgeDbContext(DbContextOptions<AuthForgeDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AuthForgeDbContext).Assembly);
    }
}
