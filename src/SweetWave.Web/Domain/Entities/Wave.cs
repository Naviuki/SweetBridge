using System.Globalization;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;

namespace SweetWave.Web.Domain.Entities;

/// <summary>
/// Сборная доставка заказов, едущих в одном направлении.
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
    /// Идентификатор волны.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Направление доставки волны.
    /// </summary>
    public ShippingDirection Direction { get; private set; }

    /// <summary>
    /// Запланированная дата отправки.
    /// </summary>
    public DateOnly ShipDate { get; private set; }

    /// <summary>
    /// Последний день приёма заказов включительно.
    /// </summary>
    public DateOnly OrderCutoffDate { get; private set; }

    /// <summary>
    /// Максимальный вес волны в граммах. Фиксируется при создании волны.
    /// </summary>
    public int CapacityGrams { get; private set; }

    /// <summary>
    /// Минимальный вес для отправки волны в граммах. Фиксируется при создании волны.
    /// </summary>
    public int MinLoadGrams { get; private set; }

    /// <summary>
    /// Состояние волны.
    /// </summary>
    public WaveStatus Status { get; private set; }

    /// <summary>
    /// Создаёт запланированную волну.
    /// </summary>
    /// <param name="direction">Направление доставки.</param>
    /// <param name="shipDate">Запланированная дата отправки.</param>
    /// <param name="orderCutoffDate">Последний день приёма заказов.</param>
    /// <param name="capacityGrams">Максимальный вес волны в граммах.</param>
    /// <param name="minLoadGrams">Минимальный вес для отправки в граммах.</param>
    /// <returns>Созданная волна.</returns>
    /// <exception cref="DomainException">Параметры волны некорректны.</exception>
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
    /// Проверяет, принимает ли волна заказы в указанный день.
    /// </summary>
    /// <param name="today">Текущая дата.</param>
    /// <returns><see langword="true"/>, если волна запланирована и дата дедлайна ещё не наступила.</returns>
    public bool IsOpenForOrders(DateOnly today) =>
        Status == WaveStatus.Planned && today <= OrderCutoffDate;

    /// <summary>
    /// Проверяет, достаточно ли свободной вместимости волны для заказа.
    /// </summary>
    /// <param name="loadedGrams">Вес, уже набранный в волне, в граммах.</param>
    /// <param name="orderGrams">Вес оформляемого заказа в граммах.</param>
    /// <returns><see langword="true"/>, если заказ помещается в оставшуюся вместимость.</returns>
    public bool CanAccept(int loadedGrams, int orderGrams) =>
        loadedGrams >= 0 && orderGrams > 0 && loadedGrams + orderGrams <= CapacityGrams;

    /// <summary>
    /// Рассчитывает заполнение волны в процентах.
    /// </summary>
    /// <param name="loadedGrams">Вес, уже набранный в волне, в граммах.</param>
    /// <returns>Заполнение волны в процентах.</returns>
    public decimal GetFillPercent(int loadedGrams) =>
        CapacityGrams == 0 ? 0m : (decimal)loadedGrams * 100m / CapacityGrams;

    /// <summary>
    /// Проверяет, можно ли отправить волну прямо сейчас.
    /// </summary>
    /// <param name="loadedGrams">Вес, уже набранный в волне, в граммах.</param>
    /// <returns><see langword="true"/>, если волна запланирована и набрана минимальная загрузка.</returns>
    public bool IsReadyToDispatch(int loadedGrams) =>
        Status == WaveStatus.Planned && loadedGrams >= MinLoadGrams;

    /// <summary>
    /// Помечает волну отправленной.
    /// </summary>
    /// <param name="loadedGrams">Вес, набранный в волне, в граммах.</param>
    /// <exception cref="DomainException">Волна уже отправлена или набрано меньше минимальной загрузки.</exception>
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
