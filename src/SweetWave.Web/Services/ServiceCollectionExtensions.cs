using SweetWave.Web.Services.Cart;
using SweetWave.Web.Services.Catalog;
using SweetWave.Web.Services.Delivery;
using SweetWave.Web.Services.Orders;
using SweetWave.Web.Services.Seasons;
using SweetWave.Web.Services.Waves;

namespace SweetWave.Web.Services;

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
