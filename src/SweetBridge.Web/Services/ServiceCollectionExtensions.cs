using SweetBridge.Web.Services.Account;
using SweetBridge.Web.Services.Auditing;
using SweetBridge.Web.Services.Cart;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Content;
using SweetBridge.Web.Services.Common;
using SweetBridge.Web.Services.Delivery;
using SweetBridge.Web.Services.Favorites;
using SweetBridge.Web.Services.Gifts;
using SweetBridge.Web.Services.Forms;
using SweetBridge.Web.Services.Media;
using SweetBridge.Web.Services.Messaging;
using SweetBridge.Web.Services.Notifications;
using SweetBridge.Web.Services.Orders;
using SweetBridge.Web.Services.Payments;
using SweetBridge.Web.Services.Promos;
using SweetBridge.Web.Services.Reporting;
using SweetBridge.Web.Services.Seasons;
using SweetBridge.Web.Services.Waves;

namespace SweetBridge.Web.Services;

/// <summary>
/// Регистрация сервисов приложения.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует все сервисы платформы.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Та же коллекция сервисов.</returns>
    public static IServiceCollection AddSweetBridgeServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IBusinessClock, BusinessClock>();
        services.AddSingleton<IDeliveryCostCalculator, DeliveryCostCalculator>();
        services.AddSingleton<ISeasonPolicy, SeasonPolicy>();
        services.AddSingleton<IContentService, ContentService>();
        services.AddSingleton<IPaymentGateway, DemoPaymentGateway>();
        services.AddSingleton<IImageStorage, ImageStorage>();

        services.AddScoped<IOrderNumberGenerator, DbOrderNumberGenerator>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IProductManagementService, ProductManagementService>();
        services.AddScoped<ICartService, SessionCartService>();
        services.AddScoped<IWaveService, WaveService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPromoService, PromoService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IFormsService, FormsService>();
        services.AddScoped<IGiftSetService, GiftSetService>();
        services.AddSingleton<IReceiptBuilder, ReceiptBuilder>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<PasswordPolicyText>();
        services.AddScoped<IEmailComposer, EmailComposer>();

        return services;
    }
}