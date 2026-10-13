using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetWave.Web.Domain.Entities;

namespace SweetWave.Web.Infrastructure.Persistence.Configurations;

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
        builder.HasIndex(wave => new { wave.Direction, wave.ShipDate });
    }
}
