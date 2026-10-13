using SweetWave.Web.Services.Cart;
using SweetWave.Web.Services.Catalog;
using SweetWave.Web.Services.Delivery;
using SweetWave.Web.Services.Orders;
using SweetWave.Web.Services.Seasons;
using SweetWave.Web.Services.Waves;

namespace SweetWave.Web.Services;

/// <summary>
/// Registration of the application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all services of the platform.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddSweetWaveServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IDeliveryCostCalculator, DeliveryCostCalculator>();
        services.AddSingleton<ISeasonPolicy, SeasonPolicy>();
        services.AddSingleton<IOrderNumberGenerator, DbOrderNumberGenerator>();

        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IProductManagementService, ProductManagementService>();
        services.AddScoped<ICartService, SessionCartService>();
        services.AddScoped<IWaveService, WaveService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}