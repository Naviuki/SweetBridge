using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Tests.Domain;

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

        Assert.True(wave.IsOpenForOrders(ToLocalTime(new DateOnly(2025, 3, 16))));
    }

    [Fact]
    public void IsOpenForOrders_OnCutoffDay_ReturnsTrue()
    {
        Wave wave = CreateWave();

        Assert.True(wave.IsOpenForOrders(ToLocalTime(CutoffDate)));
    }

    [Fact]
    public void IsOpenForOrders_DayAfterCutoff_ReturnsFalse()
    {
        Wave wave = CreateWave();

        Assert.False(wave.IsOpenForOrders(ToLocalTime(new DateOnly(2025, 3, 18))));
    }

    [Fact]
    public void IsOpenForOrders_DispatchedWave_ReturnsFalse()
    {
        Wave wave = CreateWave();
        wave.Reserve(MinLoadGrams);
        wave.MarkDispatched();

        Assert.False(wave.IsOpenForOrders(ToLocalTime(new DateOnly(2025, 3, 10))));
    }

    [Fact]
    public void CanAccept_OrderFitsIntoRemainingCapacity_ReturnsTrue()
    {
        Wave wave = CreateWave();
        wave.Reserve(250_000);

        Assert.True(wave.CanAccept(50_000));
    }

    [Fact]
    public void CanAccept_OrderExceedsRemainingCapacity_ReturnsFalse()
    {
        Wave wave = CreateWave();
        wave.Reserve(250_000);

        Assert.False(wave.CanAccept(50_001));
    }

    [Fact]
    public void Reserve_OrderExceedsCapacity_ThrowsDomainException()
    {
        Wave wave = CreateWave();
        wave.Reserve(250_000);

        DomainException exception = Assert.Throws<DomainException>(() => wave.Reserve(50_001));

        Assert.Contains("Волна", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Release_FreesReservedWeight()
    {
        Wave wave = CreateWave();
        wave.Reserve(100_000);

        wave.Release(100_000);

        Assert.Equal(0, wave.LoadedGrams);
        Assert.Equal(CapacityGrams, wave.GetRemainingCapacity());
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
        wave.Reserve(MinLoadGrams - 1);

        DomainException exception = Assert.Throws<DomainException>(wave.MarkDispatched);

        Assert.Contains("Отправить волну нельзя", exception.Message, StringComparison.Ordinal);
        Assert.Equal(WaveStatus.Planned, wave.Status);
    }

    [Fact]
    public void MarkDispatched_MinimalLoad_MarksWaveAsDispatched()
    {
        Wave wave = CreateWave();
        wave.Reserve(MinLoadGrams);

        wave.MarkDispatched();

        Assert.Equal(WaveStatus.Dispatched, wave.Status);
        Assert.False(wave.IsReadyToDispatch());
    }

    [Fact]
    public void Create_CutoffNotBeforeShipDate_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Wave.Create(
            1,
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
            1,
            ShippingDirection.DonbassToYakutia,
            ShipDate,
            CutoffDate,
            CapacityGrams,
            CapacityGrams + 1));
    }

    private static Wave CreateWave() =>
        Wave.Create(1, ShippingDirection.YakutiaToDonbass, ShipDate, CutoffDate, CapacityGrams, MinLoadGrams);

    private static DateTime ToLocalTime(DateOnly date) => date.ToDateTime(new TimeOnly(12, 0));
}