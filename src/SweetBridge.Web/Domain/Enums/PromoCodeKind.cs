namespace SweetBridge.Web.Domain.Enums;

/// <summary>
/// Тип скидки промокода.
/// </summary>
public enum PromoCodeKind
{
    /// <summary>
    /// Процент от стоимости товаров.
    /// </summary>
    Percent = 0,

    /// <summary>
    /// Фиксированная сумма в рублях.
    /// </summary>
    FixedAmount = 1
}