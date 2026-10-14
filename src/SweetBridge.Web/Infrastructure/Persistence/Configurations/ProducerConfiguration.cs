using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Infrastructure.Persistence.Configurations;

/// <summary>
/// Сопоставление для <see cref="Producer"/>.
/// </summary>
public sealed class ProducerConfiguration : IEntityTypeConfiguration<Producer>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Producer> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Producers");
        builder.HasKey(producer => producer.Id);
        builder.Property(producer => producer.Name).HasMaxLength(200).IsRequired();
        builder.Property(producer => producer.Description).HasMaxLength(1000);
        builder.HasIndex(producer => new { producer.Region, producer.IsActive });
        builder.HasIndex(producer => producer.Name).IsUnique();
    }
}