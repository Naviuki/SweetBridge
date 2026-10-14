using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Infrastructure.Persistence.Configurations;

/// <summary>
/// Сопоставление для <see cref="OrderItem"/>.
/// </summary>
public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("OrderItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ProductId).HasMaxLength(32).IsRequired();
        builder.Property(item => item.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
    }
}
