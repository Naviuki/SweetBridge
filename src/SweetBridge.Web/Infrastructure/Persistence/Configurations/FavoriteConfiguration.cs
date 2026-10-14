using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Infrastructure.Persistence.Configurations;

/// <summary>
/// Сопоставление для <see cref="Favorite"/>.
/// </summary>
public sealed class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Favorites");
        builder.HasKey(favorite => favorite.Id);
        builder.Property(favorite => favorite.ProductId).HasMaxLength(32).IsRequired();
        builder.HasIndex(favorite => new { favorite.UserId, favorite.ProductId }).IsUnique();

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(favorite => favorite.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}