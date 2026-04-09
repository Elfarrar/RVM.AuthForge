using RVM.AuthForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RVM.AuthForge.Infrastructure.Data.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApplicationApiKey>
{
    public void Configure(EntityTypeBuilder<ApplicationApiKey> builder)
    {
        builder.ToTable("api_keys");
        builder.HasKey(k => k.Id);
        builder.Property(k => k.AppId).HasMaxLength(100).IsRequired();
        builder.Property(k => k.Name).HasMaxLength(200).IsRequired();
        builder.Property(k => k.KeyHash).HasMaxLength(64).IsRequired();
        builder.Property(k => k.KeyPrefix).HasMaxLength(32).IsRequired();
        builder.HasIndex(k => k.AppId);
    }
}
