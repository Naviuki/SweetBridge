using System.Globalization;
using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Extensions;

/// <summary>
/// Русские подписи и форматы, используемые в представлениях Razor.
/// </summary>
public static class DisplayExtensions
{
    private const int GramsPerKilogram = 1000;

    private static readonly CultureInfo RussianCulture = CultureInfo.GetCultureInfo("ru-RU");

    /// <summary>
    /// Возвращает русское название региона.
    /// </summary>
    /// <param name="region">Регион производителя.</param>
    /// <returns>Русское название региона.</returns>
    public static string ToDisplayName(this Region region) => region switch
    {
        Region.Yakutia => "Якутия",
        Region.Donbass => "ДНР",
        _ => region.ToString()
    };

    /// <summary>
    /// Возвращает русское название витрины.
    /// </summary>
    /// <param name="direction">Направление перевозки.</param>
    /// <returns>Название витрины.</returns>
    public static string ToDisplayName(this ShippingDirection direction) => direction switch
    {
        ShippingDirection.YakutiaToDonbass => "Сладости Якутии",
        ShippingDirection.DonbassToYakutia => "Сладости Донбасса",
        _ => direction.ToString()
    };

    /// <summary>
    /// Возвращает короткое название витрины для заголовков и кнопок.
    /// </summary>
    /// <param name="direction">Направление перевозки.</param>
    /// <returns>Название витрины.</returns>
    public static string ToShortName(this ShippingDirection direction) => direction switch
    {
        ShippingDirection.YakutiaToDonbass => "Якутия → ДНР",
        ShippingDirection.DonbassToYakutia => "ДНР → Якутия",
        _ => direction.ToString()
    };

    /// <summary>
    /// Возвращает регион назначения для направления.
    /// </summary>
    /// <param name="direction">Направление перевозки.</param>
    /// <returns>Русское название региона назначения.</returns>
    public static string GetDestinationName(this ShippingDirection direction) => direction switch
    {
        ShippingDirection.YakutiaToDonbass => "Донецкая Народная Республика",
        ShippingDirection.DonbassToYakutia => "Якутия",
        _ => direction.ToString()
    };

    /// <summary>
    /// Возвращает русское название статуса заказа.
    /// </summary>
    /// <param name="status">Статус заказа.</param>
    /// <returns>Русское название статуса.</returns>
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
    /// Возвращает русское название статуса заказа с заглавной буквы.
    /// </summary>
    /// <param name="status">Статус заказа.</param>
    /// <returns>Русское название статуса.</returns>
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
    /// Возвращает русское название статуса волны.
    /// </summary>
    /// <param name="status">Статус волны.</param>
    /// <returns>Русское название статуса.</returns>
    public static string ToDisplayName(this WaveStatus status) => status switch
    {
        WaveStatus.Planned => "Планируется",
        WaveStatus.Dispatched => "Отправлена",
        _ => status.ToString()
    };

    /// <summary>
    /// Форматирует сумму денег в рублях.
    /// </summary>
    /// <param name="value">Сумма в рублях.</param>
    /// <returns>Отформатированная сумма, например <c>1 250 ₽</c>.</returns>
    public static string FormatPrice(this decimal value) =>
        string.Concat(value.ToString("N0", RussianCulture), " ₽");

    /// <summary>
    /// Форматирует вес в килограммах и граммах.
    /// </summary>
    /// <param name="grams">Вес в граммах.</param>
    /// <returns>Отформатированный вес, например <c>1,25 кг</c> или <c>450 г</c>.</returns>
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
    /// Форматирует процент с одним знаком после запятой при необходимости.
    /// </summary>
    /// <param name="value">Значение в процентах.</param>
    /// <returns>Отформатированный процент, например <c>50,1 %</c>.</returns>
    public static string FormatPercent(this decimal value) =>
        string.Concat(value.ToString("0.#", RussianCulture), " %");

    /// <summary>
    /// Форматирует дату в русском формате.
    /// </summary>
    /// <param name="date">Форматируемая дата.</param>
    /// <returns>Отформатированная дата, например <c>20.03.2025</c>.</returns>
    public static string FormatDate(this DateOnly date) =>
        date.ToString("dd.MM.yyyy", RussianCulture);

    /// <summary>
    /// Форматирует момент времени UTC в локальном времени сервера.
    /// </summary>
    /// <param name="dateTimeUtc">Момент времени в UTC.</param>
    /// <returns>Отформатированные дата и время.</returns>
    public static string FormatDateTime(this DateTime dateTimeUtc) =>
        dateTimeUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm", RussianCulture);
}
