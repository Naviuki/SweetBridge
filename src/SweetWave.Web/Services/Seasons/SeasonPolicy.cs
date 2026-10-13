using Microsoft.Extensions.Options;
using SweetWave.Web.Options;
using SweetWave.Web.Services.Catalog;

namespace SweetWave.Web.Services.Seasons;

/// <summary>
/// Сезонные правила на основе настроенных списков месяцев.
/// </summary>
public sealed class SeasonPolicy : ISeasonPolicy
{
    private readonly LogisticsOptions _options;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="SeasonPolicy"/>.
    /// </summary>
    /// <param name="options">Логистические параметры со списками месяцев.</param>
    public SeasonPolicy(IOptions<LogisticsOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
    }

    /// <inheritdoc />
    public bool IsThermoPackRequired(IEnumerable<ProductSummary> products, DateOnly shipDate)
    {
        ArgumentNullException.ThrowIfNull(products);

        if (!IsMonthConfigured(shipDate, _options.ThermoPackMandatoryMonths))
        {
            return false;
        }

        return products.Any(product => product.IsTemperatureSensitive);
    }

    /// <inheritdoc />
    public int GetExtraDeliveryDays(DateOnly shipDate) =>
        IsMonthConfigured(shipDate, _options.OffSeasonMonths) ? _options.OffSeasonExtraDays : 0;

    private static bool IsMonthConfigured(DateOnly date, List<int> months) => months.Contains(date.Month);
}
