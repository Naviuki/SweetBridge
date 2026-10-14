using Microsoft.Extensions.Options;
using SweetBridge.Web.Options;

namespace SweetBridge.Web.Services.Delivery;

/// <summary>
/// Расчёт стоимости доставки по настроенному тарифу, скидкам за заполнение волны и термоупаковке.
/// </summary>
public sealed class DeliveryCostCalculator : IDeliveryCostCalculator
{
    private const decimal GramsPerKilogram = 1000m;
    private const decimal PercentScale = 100m;
    private const decimal NoDiscount = 0m;

    private readonly LogisticsOptions _options;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="DeliveryCostCalculator"/>.
    /// </summary>
    /// <param name="options">Логистические параметры.</param>
    public DeliveryCostCalculator(IOptions<LogisticsOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
    }

    /// <inheritdoc />
    public DeliveryCostResult Calculate(DeliveryCostRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.WeightGrams <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                request.WeightGrams,
                "Вес заказа должен быть больше нуля.");
        }

        if (request.WaveFillPercent < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                request.WaveFillPercent,
                "Заполнение волны не может быть отрицательным.");
        }

        decimal weightKilograms = request.WeightGrams / GramsPerKilogram;
        decimal discountPercent = ResolveDiscountPercent(request.WaveFillPercent);

        decimal baseShippingCost = decimal.Round(
            weightKilograms * _options.BaseTariffPerKg,
            MidpointRounding.AwayFromZero);

        decimal shippingCost = decimal.Round(
            weightKilograms * _options.BaseTariffPerKg * (PercentScale - discountPercent) / PercentScale,
            MidpointRounding.AwayFromZero);

        decimal thermoPackCost = request.ThermoPackRequired ? _options.ThermoPackCost : NoDiscount;

        return new DeliveryCostResult(
            request.WeightGrams,
            weightKilograms,
            baseShippingCost,
            discountPercent,
            shippingCost,
            thermoPackCost,
            shippingCost + thermoPackCost);
    }

    private decimal ResolveDiscountPercent(decimal waveFillPercent) =>
        _options.FillDiscounts
            .Where(discount => waveFillPercent > discount.FillAbovePercent)
            .OrderByDescending(discount => discount.FillAbovePercent)
            .Select(discount => discount.DiscountPercent)
            .FirstOrDefault(NoDiscount);
}
