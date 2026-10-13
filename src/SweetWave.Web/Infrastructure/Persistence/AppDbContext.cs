using Microsoft.EntityFrameworkCore;
using SweetWave.Web.Domain.Entities;

namespace SweetWave.Web.Infrastructure.Persistence;

/// <summary>
/// Database context of the platform. Acts as the unit of work for all services.
/// </summary>
/// <param name="options">Context options.</param>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Catalogue products.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Consolidated delivery waves.
    /// </summary>
    public DbSet<Wave> Waves => Set<Wave>();

    /// <summary>
    /// Customer orders.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// Order positions.
    /// </summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// Order status timeline entries.
    /// </summary>
    public DbSet<OrderStatusHistoryEntry> OrderStatusHistoryEntries => Set<OrderStatusHistoryEntry>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}