using RVM.AuthForge.Domain.Entities;
using RVM.AuthForge.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RVM.AuthForge.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("audit_log");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(a => a.UserId).HasMaxLength(100).IsRequired();
        builder.Property(a => a.UserEmail).HasMaxLength(256);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(500);
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.UserId);
    }
}
