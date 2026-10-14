using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Infrastructure.Persistence.Configurations;

/// <summary>
/// Сопоставление для <see cref="GiftSet"/>.
/// </summary>
public sealed class GiftSetConfiguration : IEntityTypeConfiguration<GiftSet>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GiftSet> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("GiftSets");
        builder.HasKey(set => set.Id);
        builder.Property(set => set.Name).HasMaxLength(200).IsRequired();
        builder.Property(set => set.Description).HasMaxLength(1000);
        builder.Property(set => set.PhotoPath).HasMaxLength(200);
        builder.Property(set => set.Price).HasPrecision(18, 2);

        builder.HasMany(set => set.Items)
            .WithOne()
            .HasForeignKey(item => item.GiftSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(set => set.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

/// <summary>
/// Сопоставление для <see cref="GiftSetItem"/>.
/// </summary>
public sealed class GiftSetItemConfiguration : IEntityTypeConfiguration<GiftSetItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GiftSetItem> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("GiftSetItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ProductId).HasMaxLength(32).IsRequired();
        builder.Property(item => item.ProductName).HasMaxLength(200).IsRequired();
    }
}