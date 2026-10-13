using System.Globalization;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;

namespace SweetWave.Web.Domain.Entities;

/// <summary>
/// A consolidated delivery of orders travelling in one direction.
/// </summary>
public sealed class Wave
{
    private const decimal GramsPerKilogram = 1000m;

    private Wave()
    {
    }

    private Wave(
        ShippingDirection direction,
        DateOnly shipDate,
        DateOnly orderCutoffDate,
        int capacityGrams,
        int minLoadGrams)
    {
        Direction = direction;
        ShipDate = shipDate;
        OrderCutoffDate = orderCutoffDate;
        CapacityGrams = capacityGrams;
        MinLoadGrams = minLoadGrams;
        Status = WaveStatus.Planned;
    }

    /// <summary>
    /// Wave identifier.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Delivery direction of the wave.
    /// </summary>
    public ShippingDirection Direction { get; private set; }

    /// <summary>
    /// Planned shipment date.
    /// </summary>
    public DateOnly ShipDate { get; private set; }

    /// <summary>
    /// Last day when orders are accepted (inclusive).
    /// </summary>
    public DateOnly OrderCutoffDate { get; private set; }

    /// <summary>
    /// Maximal weight of the wave, in grams. Fixed when the wave is created.
    /// </summary>
    public int CapacityGrams { get; private set; }

    /// <summary>
    /// Minimal weight required to dispatch the wave, in grams. Fixed when the wave is created.
    /// </summary>
    public int MinLoadGrams { get; private set; }

    /// <summary>
    /// Wave state.
    /// </summary>
    public WaveStatus Status { get; private set; }

    /// <summary>
    /// Creates a planned wave.
    /// </summary>
    /// <param name="direction">Delivery direction.</param>
    /// <param name="shipDate">Planned shipment date.</param>
    /// <param name="orderCutoffDate">Last day when orders are accepted.</param>
    /// <param name="capacityGrams">Maximal weight of the wave, in grams.</param>
    /// <param name="minLoadGrams">Minimal weight required to dispatch, in grams.</param>
    /// <returns>Created wave.</returns>
    /// <exception cref="DomainException">Wave parameters are not valid.</exception>
    public static Wave Create(
        ShippingDirection direction,
        DateOnly shipDate,
        DateOnly orderCutoffDate,
        int capacityGrams,
        int minLoadGrams)
    {
        if (capacityGrams <= 0)
        {
            throw new DomainException("Вместимость волны должна быть больше нуля.");
        }

        if (minLoadGrams <= 0)
        {
            throw new DomainException("Минимальная загрузка волны должна быть больше нуля.");
        }

        if (minLoadGrams > capacityGrams)
        {
            throw new DomainException("Минимальная загрузка волны не может превышать её вместимость.");
        }

        if (orderCutoffDate >= shipDate)
        {
            throw new DomainException("Приём заказов должен заканчиваться раньше даты отправки волны.");
        }

        return new Wave(direction, shipDate, orderCutoffDate, capacityGrams, minLoadGrams);
    }

    /// <summary>
    /// Checks whether the wave still accepts orders on the given day.
    /// </summary>
    /// <param name="today">Current date.</param>
    /// <returns><see langword="true"/> when the wave is planned and the cutoff date is not reached.</returns>
    public bool IsOpenForOrders(DateOnly today) =>
        Status == WaveStatus.Planned && today <= OrderCutoffDate;

    /// <summary>
    /// Checks whether the wave has enough free capacity for the order.
    /// </summary>
    /// <param name="loadedGrams">Weight already loaded into the wave, in grams.</param>
    /// <param name="orderGrams">Weight of the order being placed, in grams.</param>
    /// <returns><see langword="true"/> when the order fits into the remaining capacity.</returns>
    public bool CanAccept(int loadedGrams, int orderGrams) =>
        loadedGrams >= 0 && orderGrams > 0 && loadedGrams + orderGrams <= CapacityGrams;

    /// <summary>
    /// Calculates the fill percentage of the wave.
    /// </summary>
    /// <param name="loadedGrams">Weight already loaded into the wave, in grams.</param>
    /// <returns>Fill percentage in percent.</returns>
    public decimal GetFillPercent(int loadedGrams) =>
        CapacityGrams == 0 ? 0m : (decimal)loadedGrams * 100m / CapacityGrams;

    /// <summary>
    /// Checks whether the wave can be dispatched right now.
    /// </summary>
    /// <param name="loadedGrams">Weight already loaded into the wave, in grams.</param>
    /// <returns><see langword="true"/> when the wave is planned and holds the minimal load.</returns>
    public bool IsReadyToDispatch(int loadedGrams) =>
        Status == WaveStatus.Planned && loadedGrams >= MinLoadGrams;

    /// <summary>
    /// Marks the wave as dispatched.
    /// </summary>
    /// <param name="loadedGrams">Weight loaded into the wave, in grams.</param>
    /// <exception cref="DomainException">Wave is already dispatched or holds less than the minimal load.</exception>
    public void MarkDispatched(int loadedGrams)
    {
        if (Status == WaveStatus.Dispatched)
        {
            throw new DomainException("Волна уже отправлена.");
        }

        if (loadedGrams < MinLoadGrams)
        {
            throw new DomainException(string.Create(
                CultureInfo.CurrentCulture,
                $"Отправить волну нельзя: собрано {loadedGrams / GramsPerKilogram:0.#} кг из требуемых {MinLoadGrams / GramsPerKilogram:0.#} кг."));
        }

        Status = WaveStatus.Dispatched;
    }
}