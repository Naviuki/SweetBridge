using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Infrastructure.Persistence.Configurations;

/// <summary>
/// Сопоставление для <see cref="Order"/>.
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Number).HasMaxLength(16).IsRequired();
        builder.HasIndex(order => order.Number).IsUnique();
        builder.HasIndex(order => new { order.Status, order.WaveId });
        builder.HasIndex(order => order.UserId);
        builder.Property(order => order.GoodsCost).HasPrecision(18, 2);
        builder.Property(order => order.DiscountAmount).HasPrecision(18, 2);
        builder.Property(order => order.ShippingCost).HasPrecision(18, 2);
        builder.Property(order => order.ThermoPackCost).HasPrecision(18, 2);
        builder.Property(order => order.FillDiscountPercent).HasPrecision(5, 2);
        builder.Property(order => order.PromoCode).HasMaxLength(32);
        builder.Property(order => order.BuyerName).HasMaxLength(200).IsRequired();
        builder.Property(order => order.BuyerPhone).HasMaxLength(40).IsRequired();
        builder.Property(order => order.RecipientName).HasMaxLength(200).IsRequired();
        builder.Property(order => order.RecipientPhone).HasMaxLength(40).IsRequired();
        builder.Property(order => order.RecipientCity).HasMaxLength(200).IsRequired();
        builder.Property(order => order.RecipientPickupPoint).HasMaxLength(300);
        builder.Property(order => order.GiftMessage).HasMaxLength(300);

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(order => order.History)
            .WithOne()
            .HasForeignKey(entry => entry.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Wave>()
            .WithMany()
            .HasForeignKey(order => order.WaveId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PickupPoint>()
            .WithMany()
            .HasForeignKey(order => order.PickupPointId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(order => order.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(order => order.History).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}