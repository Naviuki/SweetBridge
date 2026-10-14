using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;

namespace SweetBridge.Web.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных платформы. Выступает единицей работы для всех сервисов.
/// </summary>
/// <param name="options">Параметры контекста.</param>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole, string>(options)
{
    /// <summary>
    /// Товары каталога.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Производители товаров.
    /// </summary>
    public DbSet<Producer> Producers => Set<Producer>();

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

    /// <summary>
    /// Позиции корзины авторизованного пользователя.
    /// </summary>
    public DbSet<CartItem> CartItems => Set<CartItem>();

    /// <summary>
    /// Избранные товары.
    /// </summary>
    public DbSet<Favorite> Favorites => Set<Favorite>();

    /// <summary>
    /// Промокоды.
    /// </summary>
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();

    /// <summary>
    /// Уведомления пользователей.
    /// </summary>
    public DbSet<Notification> Notifications => Set<Notification>();

    /// <summary>
    /// Пункты выдачи.
    /// </summary>
    public DbSet<PickupPoint> PickupPoints => Set<PickupPoint>();

    /// <summary>
    /// Подарочные наборы.
    /// </summary>
    public DbSet<GiftSet> GiftSets => Set<GiftSet>();

    /// <summary>
    /// Состав подарочных наборов.
    /// </summary>
    public DbSet<GiftSetItem> GiftSetItems => Set<GiftSetItem>();

    /// <summary>
    /// Обращения в поддержку.
    /// </summary>
    public DbSet<SupportRequest> SupportRequests => Set<SupportRequest>();

    /// <summary>
    /// Корпоративные заявки.
    /// </summary>
    public DbSet<CorporateRequest> CorporateRequests => Set<CorporateRequest>();

    /// <summary>
    /// Подписчики рассылки.
    /// </summary>
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();

    /// <summary>
    /// Журнал действий администратора.
    /// </summary>
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}