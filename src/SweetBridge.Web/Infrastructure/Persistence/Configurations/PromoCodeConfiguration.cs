using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Infrastructure.Persistence.Configurations;

/// <summary>
/// Сопоставление для <see cref="PromoCode"/>.
/// </summary>
public sealed class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("PromoCodes");
        builder.HasKey(promo => promo.Id);
        builder.Property(promo => promo.Code).HasMaxLength(32).IsRequired();
        builder.HasIndex(promo => promo.Code).IsUnique();
        builder.Property(promo => promo.Value).HasPrecision(18, 2);
        builder.Property(promo => promo.MinOrderAmount).HasPrecision(18, 2);
    }
}