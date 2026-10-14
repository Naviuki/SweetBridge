using System.ComponentModel.DataAnnotations;

namespace SweetBridge.Web.Options;

/// <summary>
/// Логистические параметры платформы. Все числа доставки берутся из конфигурации.
/// </summary>
public sealed class LogisticsOptions
{
    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public const string SectionName = "Logistics";

    /// <summary>
    /// Базовый тариф за килограмм посылки в рублях.
    /// </summary>
    [Range(1, 100_000)]
    public decimal BaseTariffPerKg { get; set; }

    /// <summary>
    /// Стоимость термоупаковки в рублях.
    /// </summary>
    [Range(0, 100_000)]
    public decimal ThermoPackCost { get; set; }

    /// <summary>
    /// Интервал между волнами одного направления в днях.
    /// </summary>
    [Range(1, 365)]
    public int WaveIntervalDays { get; set; }

    /// <summary>
    /// За сколько дней до даты отправки заказы перестают приниматься.
    /// </summary>
    [Range(0, 60)]
    public int OrderCutoffDaysBeforeShipment { get; set; }

    /// <summary>
    /// Минимальный общий вес отправляемой волны в килограммах.
    /// </summary>
    [Range(1, 100_000)]
    public int MinWaveLoadKg { get; set; }

    /// <summary>
    /// Максимальный общий вес волны в килограммах.
    /// </summary>
    [Range(1, 1_000_000)]
    public int MaxWaveLoadKg { get; set; }

    /// <summary>
    /// Минимальный срок доставки в днях.
    /// </summary>
    [Range(1, 365)]
    public int MinDeliveryDays { get; set; }

    /// <summary>
    /// Максимальный срок доставки в днях.
    /// </summary>
    [Range(1, 365)]
    public int MaxDeliveryDays { get; set; }

    /// <summary>
    /// Дополнительные дни, добавляемые к сроку доставки в межсезонье.
    /// </summary>
    [Range(0, 365)]
    public int OffSeasonExtraDays { get; set; }

    /// <summary>
    /// Месяцы (1–12) с увеличенным сроком доставки.
    /// </summary>
    [MinLength(1)]
    public List<int> OffSeasonMonths { get; set; } = [];

    /// <summary>
    /// Месяцы (1–12), когда термоупаковка обязательна для чувствительных товаров.
    /// </summary>
    [MinLength(1)]
    public List<int> ThermoPackMandatoryMonths { get; set; } = [];

    /// <summary>
    /// Пороги скидок по заполнению волны.
    /// </summary>
    [MinLength(1)]
    public List<FillDiscountOption> FillDiscounts { get; set; } = [];
}
