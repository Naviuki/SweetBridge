namespace SweetWave.Web.Services.Delivery;

/// <summary>
/// Result of the delivery cost calculation.
/// </summary>
/// <param name="WeightGrams">Weight of the order, in grams.</param>
/// <param name="WeightKilograms">Weight of the order, in kilograms.</param>
/// <param name="BaseShippingCost">Delivery cost without the fill discount, in rubles.</param>
/// <param name="DiscountPercent">Discount applied to the delivery cost, in percent.</param>
/// <param name="ShippingCost">Delivery cost with the discount, in rubles.</param>
/// <param name="ThermoPackCost">Thermal packaging cost, in rubles.</param>
/// <param name="TotalCost">Delivery plus thermal packaging cost, in rubles.</param>
public sealed record DeliveryCostResult(
    int WeightGrams,
    decimal WeightKilograms,
    decimal BaseShippingCost,
    decimal DiscountPercent,
    decimal ShippingCost,
    decimal ThermoPackCost,
    decimal TotalCost);