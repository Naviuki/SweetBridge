using SweetWave.Web.Services.Delivery;
using SweetWave.Web.Services.Seasons;

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

        return services;
    }
}