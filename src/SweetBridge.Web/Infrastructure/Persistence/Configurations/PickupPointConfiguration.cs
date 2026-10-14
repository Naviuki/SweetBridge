using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Infrastructure.Persistence.Configurations;

/// <summary>
/// Сопоставление для <see cref="PickupPoint"/>.
/// </summary>
public sealed class PickupPointConfiguration : IEntityTypeConfiguration<PickupPoint>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PickupPoint> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("PickupPoints");
        builder.HasKey(point => point.Id);
        builder.Property(point => point.City).HasMaxLength(200).IsRequired();
        builder.Property(point => point.Number).HasMaxLength(50).IsRequired();
        builder.Property(point => point.Address).HasMaxLength(300).IsRequired();
        builder.HasIndex(point => new { point.Region, point.IsActive });
    }
}