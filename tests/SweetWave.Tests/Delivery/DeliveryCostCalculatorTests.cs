using SweetWave.Web.Options;
using SweetWave.Web.Services.Delivery;

namespace SweetWave.Tests.Delivery;

/// <summary>
/// Tests of <see cref="DeliveryCostCalculator"/>.
/// </summary>
public sealed class DeliveryCostCalculatorTests
{
    private const decimal TariffPerKg = 500m;
    private const decimal ThermoPackCost = 150m;
    private const int WeightTwoKilograms = 2000;

    [Fact]
    public void Calculate_FillBelowFirstThreshold_ReturnsFullTariff()
    {
        DeliveryCostCalculator calculator = CreateCalculator();

        DeliveryCostResult result = calculator.Calculate(new DeliveryCostRequest(WeightTwoKilograms, 49.9m, false));

        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(1000m, result.BaseShippingCost);
        Assert.Equal(1000m, result.ShippingCost);
        Assert.Equal(0m, result.ThermoPackCost);
        Assert.Equal(1000m, result.TotalCost);
    }

    [Fact]
    public void Calculate_FillExactlyFiftyPercent_ReturnsFullTariff()
    {
        DeliveryCostCalculator calculator = CreateCalculator();

        DeliveryCostResult result = calculator.Calculate(new DeliveryCostRequest(WeightTwoKilograms, 50m, false));

        Assert.Equal(0m, result.DiscountPercent);
        Assert.Equal(1000m, result.ShippingCost);
    }

    [Fact]
    public void Calculate_FillJustAboveFiftyPercent_AppliesTwentyPercentDiscount()
    {
        DeliveryCostCalculator calculator = CreateCalculator();

        DeliveryCostResult result = calculator.Calculate(new DeliveryCostRequest(WeightTwoKilograms, 50.1m, false));

        Assert.Equal(20m, result.DiscountPercent);
        Assert.Equal(800m, result.ShippingCost);
    }

    [Fact]
    public void Calculate_FillExactlyEightyPercent_AppliesTwentyPercentDiscount()
    {
        DeliveryCostCalculator calculator = CreateCalculator();

        DeliveryCostResult result = calculator.Calculate(new DeliveryCostRequest(WeightTwoKilograms, 80m, false));

        Assert.Equal(20m, result.DiscountPercent);
        Assert.Equal(800m, result.ShippingCost);
    }

    [Fact]
    public void Calculate_FillJustAboveEightyPercent_AppliesThirtyFivePercentDiscount()
    {
        DeliveryCostCalculator calculator = CreateCalculator();

        DeliveryCostResult result = calculator.Calculate(new DeliveryCostRequest(WeightTwoKilograms, 80.1m, false));

        Assert.Equal(35m, result.DiscountPercent);
        Assert.Equal(650m, result.ShippingCost);
    }

    [Fact]
    public void Calculate_ThermoPackRequired_AddsThermoPackCostToTotal()
    {
        DeliveryCostCalculator calculator = CreateCalculator();

        DeliveryCostResult result = calculator.Calculate(new DeliveryCostRequest(WeightTwoKilograms, 60m, true));

        Assert.Equal(800m, result.ShippingCost);
        Assert.Equal(150m, result.ThermoPackCost);
        Assert.Equal(950m, result.TotalCost);
    }

    [Fact]
    public void Calculate_WeightWithHalfRuble_RoundsAwayFromZero()
    {
        DeliveryCostCalculator calculator = CreateCalculator();

        DeliveryCostResult result = calculator.Calculate(new DeliveryCostRequest(1001, 10m, false));

        Assert.Equal(1.001m, result.WeightKilograms);
        Assert.Equal(501m, result.BaseShippingCost);
        Assert.Equal(501m, result.ShippingCost);
    }

    [Fact]
    public void Calculate_ZeroWeight_ThrowsArgumentOutOfRangeException()
    {
        DeliveryCostCalculator calculator = CreateCalculator();

        Assert.Throws<ArgumentOutOfRangeException>(() => calculator.Calculate(new DeliveryCostRequest(0, 10m, false)));
    }

    private static DeliveryCostCalculator CreateCalculator()
    {
        LogisticsOptions options = new()
        {
            BaseTariffPerKg = TariffPerKg,
            ThermoPackCost = ThermoPackCost,
            FillDiscounts =
            [
                new FillDiscountOption { FillAbovePercent = 80m, DiscountPercent = 35m },
                new FillDiscountOption { FillAbovePercent = 50m, DiscountPercent = 20m }
            ]
        };

        return new DeliveryCostCalculator(Microsoft.Extensions.Options.Options.Create(options));
    }
}