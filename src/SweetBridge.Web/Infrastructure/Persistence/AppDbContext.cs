using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных платформы. Выступает единицей работы для всех сервисов.
/// </summary>
/// <param name="options">Параметры контекста.</param>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Товары каталога.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Сборные волны доставки.
    /// </summary>
    public DbSet<Wave> Waves => Set<Wave>();

    /// <summary>
    /// Заказы покупателей.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// Позиции заказа.
    /// </summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// Записи ленты смены статусов заказа.
    /// </summary>
    public DbSet<OrderStatusHistoryEntry> OrderStatusHistoryEntries => Set<OrderStatusHistoryEntry>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
