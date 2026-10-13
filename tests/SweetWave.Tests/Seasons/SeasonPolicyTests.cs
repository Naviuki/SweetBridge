using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Options;
using SweetWave.Web.Services.Catalog;
using SweetWave.Web.Services.Seasons;

namespace SweetWave.Tests.Seasons;

/// <summary>
/// Тесты <see cref="SeasonPolicy"/>: покрыты все месяцы года.
/// </summary>
public sealed class SeasonPolicyTests
{
    private const int ReferenceYear = 2025;
    private const int ExtraDays = 7;

    public static TheoryData<int, bool> AllMonthsWithMandatoryThermoPack => new()
    {
        { 1, true },
        { 2, true },
        { 3, false },
        { 4, false },
        { 5, false },
        { 6, true },
        { 7, true },
        { 8, true },
        { 9, false },
        { 10, false },
        { 11, false },
        { 12, true }
    };

    public static TheoryData<int, int> AllMonthsWithExtraDeliveryDays => new()
    {
        { 1, 0 },
        { 2, 0 },
        { 3, 0 },
        { 4, 0 },
        { 5, ExtraDays },
        { 6, 0 },
        { 7, 0 },
        { 8, 0 },
        { 9, 0 },
        { 10, 0 },
        { 11, ExtraDays },
        { 12, 0 }
    };

    [Theory]
    [MemberData(nameof(AllMonthsWithMandatoryThermoPack))]
    public void IsThermoPackRequired_TemperatureSensitiveProduct_MatchesMandatoryMonths(int month, bool expected)
    {
        SeasonPolicy policy = CreatePolicy();
        ProductSummary product = CreateProduct(isTemperatureSensitive: true);

        bool result = policy.IsThermoPackRequired([product], new DateOnly(ReferenceYear, month, 15));

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(12)]
    public void IsThermoPackRequired_RegularProductInColdOrHotMonth_ReturnsFalse(int month)
    {
        SeasonPolicy policy = CreatePolicy();
        ProductSummary product = CreateProduct(isTemperatureSensitive: false);

        bool result = policy.IsThermoPackRequired([product], new DateOnly(ReferenceYear, month, 15));

        Assert.False(result);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(11)]
    public void IsThermoPackRequired_TemperatureSensitiveProductOutsideMandatoryMonths_ReturnsFalse(int month)
    {
        SeasonPolicy policy = CreatePolicy();
        ProductSummary product = CreateProduct(isTemperatureSensitive: true);

        bool result = policy.IsThermoPackRequired([product], new DateOnly(ReferenceYear, month, 15));

        Assert.False(result);
    }

    [Theory]
    [MemberData(nameof(AllMonthsWithExtraDeliveryDays))]
    public void GetExtraDeliveryDays_EveryMonthOfYear_MatchesOffSeasonMonths(int month, int expected)
    {
        SeasonPolicy policy = CreatePolicy();

        int result = policy.GetExtraDeliveryDays(new DateOnly(ReferenceYear, month, 10));

        Assert.Equal(expected, result);
    }

    private static SeasonPolicy CreatePolicy()
    {
        LogisticsOptions options = new()
        {
            OffSeasonExtraDays = ExtraDays,
            OffSeasonMonths = [5, 11],
            ThermoPackMandatoryMonths = [12, 1, 2, 6, 7, 8]
        };

        return new SeasonPolicy(Microsoft.Extensions.Options.Options.Create(options));
    }

    private static ProductSummary CreateProduct(bool isTemperatureSensitive) =>
        new(
            "YKT-001",
            Region.Yakutia,
            "ЯХК",
            "Тестовый шоколад",
            "Шоколад",
            100,
            250m,
            isTemperatureSensitive,
            180,
            null,
            null,
            ShippingDirection.YakutiaToDonbass,
            true);
}
