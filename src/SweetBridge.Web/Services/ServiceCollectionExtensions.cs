using SweetBridge.Web.Services.Cart;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Delivery;
using SweetBridge.Web.Services.Orders;
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
    public static IServiceCollection AddSweetWaveServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IDeliveryCostCalculator, DeliveryCostCalculator>();
        services.AddSingleton<ISeasonPolicy, SeasonPolicy>();

        services.AddScoped<IOrderNumberGenerator, DbOrderNumberGenerator>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IProductManagementService, ProductManagementService>();
        services.AddScoped<ICartService, SessionCartService>();
        services.AddScoped<IWaveService, WaveService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
