using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Промокод на скидку за товары (не на доставку).
/// </summary>
public sealed class PromoCode
{
    private PromoCode()
    {
    }

    private PromoCode(
        string code,
        PromoCodeKind kind,
        decimal value,
        DateOnly validFrom,
        DateOnly validTo,
        int maxUses,
        decimal minOrderAmount)
    {
        Code = code;
        Kind = kind;
        Value = value;
        ValidFrom = validFrom;
        ValidTo = validTo;
        MaxUses = maxUses;
        MinOrderAmount = minOrderAmount;
        IsActive = true;
    }

    /// <summary>
    /// Идентификатор промокода.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Код промокода в верхнем регистре.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Тип скидки.
    /// </summary>
    public PromoCodeKind Kind { get; private set; }

    /// <summary>
    /// Размер скидки: процент или сумма в рублях.
    /// </summary>
    public decimal Value { get; private set; }

    /// <summary>
    /// Дата начала действия включительно.
    /// </summary>
    public DateOnly ValidFrom { get; private set; }

    /// <summary>
    /// Дата окончания действия включительно.
    /// </summary>
    public DateOnly ValidTo { get; private set; }

    /// <summary>
    /// Максимальное количество использований, ноль означает без ограничений.
    /// </summary>
    public int MaxUses { get; private set; }

    /// <summary>
    /// Текущее количество использований.
    /// </summary>
    public int UsedCount { get; private set; }

    /// <summary>
    /// Минимальная сумма товаров для применения.
    /// </summary>
    public decimal MinOrderAmount { get; private set; }

    /// <summary>
    /// Признак активности.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Создаёт промокод.
    /// </summary>
    /// <param name="code">Код.</param>
    /// <param name="kind">Тип скидки.</param>
    /// <param name="value">Размер скидки.</param>
    /// <param name="validFrom">Начало действия.</param>
    /// <param name="validTo">Окончание действия.</param>
    /// <param name="maxUses">Лимит использований, 0 — без ограничений.</param>
    /// <param name="minOrderAmount">Минимальная сумма товаров.</param>
    /// <returns>Новый промокод.</returns>
    /// <exception cref="DomainException">Параметры промокода некорректны.</exception>
    public static PromoCode Create(
        string code,
        PromoCodeKind kind,
        decimal value,
        DateOnly validFrom,
        DateOnly validTo,
        int maxUses,
        decimal minOrderAmount)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Укажите код промокода.");
        }

        if (value <= 0m)
        {
            throw new DomainException("Размер скидки должен быть больше нуля.");
        }

        if (kind == PromoCodeKind.Percent && value > 100m)
        {
            throw new DomainException("Процентная скидка не может быть больше 100.");
        }

        if (validTo < validFrom)
        {
            throw new DomainException("Дата окончания не может быть раньше даты начала.");
        }

        if (maxUses < 0 || minOrderAmount < 0m)
        {
            throw new DomainException("Лимит использований и минимальная сумма не могут быть отрицательными.");
        }

        return new PromoCode(code.Trim().ToUpperInvariant(), kind, value, validFrom, validTo, maxUses, minOrderAmount);
    }

    /// <summary>
    /// Проверяет применимость промокода на указанную дату и сумму товаров.
    /// </summary>
    /// <param name="today">Текущая дата.</param>
    /// <param name="goodsTotal">Сумма товаров в корзине.</param>
    /// <returns><see langword="true"/>, если промокод можно применить.</returns>
    public bool CanBeApplied(DateOnly today, decimal goodsTotal) =>
        IsActive
        && today >= ValidFrom
        && today <= ValidTo
        && (MaxUses == 0 || UsedCount < MaxUses)
        && goodsTotal >= MinOrderAmount;

    /// <summary>
    /// Рассчитывает скидку для суммы товаров.
    /// </summary>
    /// <param name="goodsTotal">Сумма товаров.</param>
    /// <returns>Размер скидки, но не больше суммы товаров.</returns>
    public decimal CalculateDiscount(decimal goodsTotal) => Kind switch
    {
        PromoCodeKind.Percent => Math.Round(goodsTotal * Value / 100m, 2, MidpointRounding.AwayFromZero),
        PromoCodeKind.FixedAmount => Math.Min(Value, goodsTotal),
        _ => 0m
    };

    /// <summary>
    /// Фиксирует применение промокода.
    /// </summary>
    public void RegisterUsage() => UsedCount++;

    /// <summary>
    /// Деактивирует промокод.
    /// </summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Возвращает промокод в активные.
    /// </summary>
    public void Activate() => IsActive = true;
}