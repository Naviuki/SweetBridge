using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Infrastructure.Persistence.Configurations;

/// <summary>
/// Сопоставление для <see cref="Wave"/>.
/// </summary>
public sealed class WaveConfiguration : IEntityTypeConfiguration<Wave>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Wave> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Waves");
        builder.HasKey(wave => wave.Id);
        builder.HasIndex(wave => wave.Number).IsUnique();
        builder.HasIndex(wave => new { wave.Direction, wave.ShipDate });

        builder.Property(wave => wave.Version).IsRowVersion();

        builder.HasMany<Order>()
            .WithOne()
            .HasForeignKey(order => order.WaveId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}