using Microsoft.Extensions.Options;
using SweetWave.Web.Domain.Entities;
using SweetWave.Web.Options;

namespace SweetWave.Web.Services.Seasons;

/// <summary>
/// Seasonal rules driven by the configured month lists.
/// </summary>
public sealed class SeasonPolicy : ISeasonPolicy
{
    private readonly LogisticsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeasonPolicy"/> class.
    /// </summary>
    /// <param name="options">Logistics parameters with the month lists.</param>
    public SeasonPolicy(IOptions<LogisticsOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
    }

    /// <inheritdoc />
    public bool IsThermoPackRequired(IEnumerable<Product> products, DateOnly shipDate)
    {
        ArgumentNullException.ThrowIfNull(products);

        if (!IsMonthConfigured(shipDate, _options.ThermoPackMandatoryMonths))
        {
            return false;
        }

        return products.Any(product => product is { IsTemperatureSensitive: true });
    }

    /// <inheritdoc />
    public int GetExtraDeliveryDays(DateOnly shipDate) =>
        IsMonthConfigured(shipDate, _options.OffSeasonMonths) ? _options.OffSeasonExtraDays : 0;

    private static bool IsMonthConfigured(DateOnly date, List<int> months) => months.Contains(date.Month);
}