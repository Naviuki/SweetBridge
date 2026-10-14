namespace SweetBridge.Web.Services.Delivery;

/// <summary>
/// Результат расчёта стоимости доставки.
/// </summary>
/// <param name="WeightGrams">Вес заказа в граммах.</param>
/// <param name="WeightKilograms">Вес заказа в килограммах.</param>
/// <param name="BaseShippingCost">Стоимость доставки без скидки за заполнение в рублях.</param>
/// <param name="DiscountPercent">Скидка на стоимость доставки в процентах.</param>
/// <param name="ShippingCost">Стоимость доставки с учётом скидки в рублях.</param>
/// <param name="thermoPackCost">Стоимость термоупаковки в рублях.</param>
/// <param name="TotalCost">Доставка вместе с термоупаковкой в рублях.</param>
public sealed record DeliveryCostResult(
    int WeightGrams,
    decimal WeightKilograms,
    decimal BaseShippingCost,
    decimal DiscountPercent,
    decimal ShippingCost,
    decimal ThermoPackCost,
    decimal TotalCost);
