using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetWave.Web.Domain.Entities;

namespace SweetWave.Web.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapping of <see cref="Product"/>.
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).HasMaxLength(32);
        builder.Property(product => product.Manufacturer).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Category).HasMaxLength(100).IsRequired();
        builder.Property(product => product.Price).HasPrecision(18, 2);
        builder.Property(product => product.Photo).HasMaxLength(200);
        builder.Property(product => product.Description).HasMaxLength(1000);
        builder.HasIndex(product => new { product.Region, product.IsActive });
    }
}