using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Infrastructure.Persistence.Configurations;

/// <summary>
/// Сопоставление для <see cref="OrderStatusHistoryEntry"/>.
/// </summary>
public sealed class OrderStatusHistoryEntryConfiguration : IEntityTypeConfiguration<OrderStatusHistoryEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderStatusHistoryEntry> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("OrderStatusHistory");
        builder.HasKey(entry => entry.Id);
        builder.HasIndex(entry => entry.OrderId);
    }
}
