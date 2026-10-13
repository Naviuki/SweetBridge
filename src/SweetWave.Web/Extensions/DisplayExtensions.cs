using System.Globalization;
using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Extensions;

/// <summary>
/// Russian labels and formats used by the Razor views.
/// </summary>
public static class DisplayExtensions
{
    private const int GramsPerKilogram = 1000;

    private static readonly CultureInfo RussianCulture = CultureInfo.GetCultureInfo("ru-RU");

    /// <summary>
    /// Returns the Russian name of the region.
    /// </summary>
    /// <param name="region">Region of a manufacturer.</param>
    /// <returns>Russian name of the region.</returns>
    public static string ToDisplayName(this Region region) => region switch
    {
        Region.Yakutia => "Якутия",
        Region.Donbass => "ДНР",
        _ => region.ToString()
    };

    /// <summary>
    /// Returns the Russian name of the showcase.
    /// </summary>
    /// <param name="direction">Shipping direction.</param>
    /// <returns>Showcase name.</returns>
    public static string ToDisplayName(this ShippingDirection direction) => direction switch
    {
        ShippingDirection.YakutiaToDonbass => "Сладости Якутии",
        ShippingDirection.DonbassToYakutia => "Сладости Донбасса",
        _ => direction.ToString()
    };

    /// <summary>
    /// Returns the short name of the showcase used in titles and buttons.
    /// </summary>
    /// <param name="direction">Shipping direction.</param>
    /// <returns>Showcase name.</returns>
    public static string ToShortName(this ShippingDirection direction) => direction switch
    {
        ShippingDirection.YakutiaToDonbass => "Якутия → ДНР",
        ShippingDirection.DonbassToYakutia => "ДНР → Якутия",
        _ => direction.ToString()
    };

    /// <summary>
    /// Returns the destination region of the direction.
    /// </summary>
    /// <param name="direction">Shipping direction.</param>
    /// <returns>Russian name of the destination region.</returns>
    public static string GetDestinationName(this ShippingDirection direction) => direction switch
    {
        ShippingDirection.YakutiaToDonbass => "Донецкая Народная Республика",
        ShippingDirection.DonbassToYakutia => "Якутия",
        _ => direction.ToString()
    };

    /// <summary>
    /// Returns the Russian name of the order status.
    /// </summary>
    /// <param name="status">Order status.</param>
    /// <returns>Russian name of the status.</returns>
    public static string ToDisplayName(this OrderStatus status) => status switch
    {
        OrderStatus.Accepted => "принят",
        OrderStatus.Assembling => "в сборке",
        OrderStatus.InTransit => "в пути",
        OrderStatus.ReadyForPickup => "на пункте выдачи",
        OrderStatus.Received => "получен",
        _ => status.ToString()
    };

    /// <summary>
    /// Returns the Russian name of the order status starting with a capital letter.
    /// </summary>
    /// <param name="status">Order status.</param>
    /// <returns>Russian name of the status.</returns>
    public static string ToDisplayTitle(this OrderStatus status) => status switch
    {
        OrderStatus.Accepted => "Принят",
        OrderStatus.Assembling => "В сборке",
        OrderStatus.InTransit => "В пути",
        OrderStatus.ReadyForPickup => "На пункте выдачи",
        OrderStatus.Received => "Получен",
        _ => status.ToString()
    };

    /// <summary>
    /// Returns the Russian name of the wave status.
    /// </summary>
    /// <param name="status">Wave status.</param>
    /// <returns>Russian name of the status.</returns>
    public static string ToDisplayName(this WaveStatus status) => status switch
    {
        WaveStatus.Planned => "Планируется",
        WaveStatus.Dispatched => "Отправлена",
        _ => status.ToString()
    };

    /// <summary>
    /// Formats an amount of money in rubles.
    /// </summary>
    /// <param name="value">Amount in rubles.</param>
    /// <returns>Formatted amount, for example <c>1 250 ₽</c>.</returns>
    public static string FormatPrice(this decimal value) =>
        string.Concat(value.ToString("N0", RussianCulture), " ₽");

    /// <summary>
    /// Formats a weight in kilograms with grams.
    /// </summary>
    /// <param name="grams">Weight in grams.</param>
    /// <returns>Formatted weight, for example <c>1,25 кг</c> or <c>450 г</c>.</returns>
    public static string FormatWeight(this int grams)
    {
        if (grams < GramsPerKilogram)
        {
            return string.Concat(grams.ToString("N0", RussianCulture), " г");
        }

        decimal kilograms = grams / (decimal)GramsPerKilogram;

        return string.Concat(kilograms.ToString("0.##", RussianCulture), " кг");
    }

    /// <summary>
    /// Formats a percentage with one decimal when needed.
    /// </summary>
    /// <param name="value">Percentage value.</param>
    /// <returns>Formatted percentage, for example <c>50,1 %</c>.</returns>
    public static string FormatPercent(this decimal value) =>
        string.Concat(value.ToString("0.#", RussianCulture), " %");

    /// <summary>
    /// Formats a date in the Russian format.
    /// </summary>
    /// <param name="date">Date to format.</param>
    /// <returns>Formatted date, for example <c>20.03.2025</c>.</returns>
    public static string FormatDate(this DateOnly date) =>
        date.ToString("dd.MM.yyyy", RussianCulture);

    /// <summary>
    /// Formats a UTC moment in the local time of the server.
    /// </summary>
    /// <param name="dateTimeUtc">Moment in UTC.</param>
    /// <returns>Formatted date and time.</returns>
    public static string FormatDateTime(this DateTime dateTimeUtc) =>
        dateTimeUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm", RussianCulture);
}