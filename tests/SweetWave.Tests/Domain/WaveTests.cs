using SweetWave.Web.Domain.Entities;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;

namespace SweetWave.Tests.Domain;

/// <summary>
/// Тесты <see cref="Wave"/>: даты приёма заказов и правила вместимости.
/// </summary>
public sealed class WaveTests
{
    private static readonly DateOnly ShipDate = new(2025, 3, 20);
    private static readonly DateOnly CutoffDate = new(2025, 3, 17);
    private const int CapacityGrams = 300_000;
    private const int MinLoadGrams = 50_000;

    [Fact]
    public void IsOpenForOrders_DayBeforeCutoff_ReturnsTrue()
    {
        Wave wave = CreateWave();

        Assert.True(wave.IsOpenForOrders(new DateOnly(2025, 3, 16)));
    }

    [Fact]
    public void IsOpenForOrders_OnCutoffDay_ReturnsTrue()
    {
        Wave wave = CreateWave();

        Assert.True(wave.IsOpenForOrders(CutoffDate));
    }

    [Fact]
    public void IsOpenForOrders_DayAfterCutoff_ReturnsFalse()
    {
        Wave wave = CreateWave();

        Assert.False(wave.IsOpenForOrders(new DateOnly(2025, 3, 18)));
    }

    [Fact]
    public void IsOpenForOrders_DispatchedWave_ReturnsFalse()
    {
        Wave wave = CreateWave();
        wave.MarkDispatched(MinLoadGrams);

        Assert.False(wave.IsOpenForOrders(new DateOnly(2025, 3, 10)));
    }

    [Fact]
    public void CanAccept_OrderFitsIntoRemainingCapacity_ReturnsTrue()
    {
        Wave wave = CreateWave();

        Assert.True(wave.CanAccept(250_000, 50_000));
    }

    [Fact]
    public void CanAccept_OrderExceedsRemainingCapacity_ReturnsFalse()
    {
        Wave wave = CreateWave();

        Assert.False(wave.CanAccept(250_000, 50_001));
    }

    [Fact]
    public void GetFillPercent_HalfOfCapacity_ReturnsFifty()
    {
        Wave wave = CreateWave();

        Assert.Equal(50m, wave.GetFillPercent(150_000));
    }

    [Fact]
    public void MarkDispatched_LessThanMinimalLoad_ThrowsDomainException()
    {
        Wave wave = CreateWave();

        DomainException exception = Assert.Throws<DomainException>(() => wave.MarkDispatched(MinLoadGrams - 1));

        Assert.Contains("Отправить волну нельзя", exception.Message, StringComparison.Ordinal);
        Assert.Equal(WaveStatus.Planned, wave.Status);
    }

    [Fact]
    public void MarkDispatched_MinimalLoad_MarksWaveAsDispatched()
    {
        Wave wave = CreateWave();

        wave.MarkDispatched(MinLoadGrams);

        Assert.Equal(WaveStatus.Dispatched, wave.Status);
        Assert.False(wave.IsReadyToDispatch(MinLoadGrams));
    }

    [Fact]
    public void Create_CutoffNotBeforeShipDate_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Wave.Create(
            ShippingDirection.YakutiaToDonbass,
            ShipDate,
            ShipDate,
            CapacityGrams,
            MinLoadGrams));
    }

    [Fact]
    public void Create_MinLoadAboveCapacity_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Wave.Create(
            ShippingDirection.DonbassToYakutia,
            ShipDate,
            CutoffDate,
            CapacityGrams,
            CapacityGrams + 1));
    }

    private static Wave CreateWave() =>
        Wave.Create(ShippingDirection.YakutiaToDonbass, ShipDate, CutoffDate, CapacityGrams, MinLoadGrams);
}
