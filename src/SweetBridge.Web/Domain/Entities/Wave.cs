using System.Globalization;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Сборная волна доставки в одном направлении.
/// </summary>
public sealed class Wave
{
    private const decimal GramsPerKilogram = 1000m;

    private Wave()
    {
    }

    private Wave(
        int number,
        ShippingDirection direction,
        DateOnly shipDate,
        DateOnly orderCutoffDate,
        int capacityGrams,
        int minLoadGrams)
    {
        Number = number;
        Direction = direction;
        ShipDate = shipDate;
        OrderCutoffDate = orderCutoffDate;
        CapacityGrams = capacityGrams;
        MinLoadGrams = minLoadGrams;
        LoadedGrams = 0;
        Status = WaveStatus.Planned;
    }

    /// <summary>
    /// Идентификатор волны.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Сквозной номер волны для показа пользователю.
    /// </summary>
    public int Number { get; private set; }

    /// <summary>
    /// Направление доставки.
    /// </summary>
    public ShippingDirection Direction { get; private set; }

    /// <summary>
    /// Дата отправки.
    /// </summary>
    public DateOnly ShipDate { get; private set; }

    /// <summary>
    /// Последний день приёма заказов включительно (по местному времени региона).
    /// </summary>
    public DateOnly OrderCutoffDate { get; private set; }

    /// <summary>
    /// Вместимость волны в граммах.
    /// </summary>
    public int CapacityGrams { get; private set; }

    /// <summary>
    /// Минимальная загрузка для отправки в граммах.
    /// </summary>
    public int MinLoadGrams { get; private set; }

    /// <summary>
    /// Текущий набранный вес в граммах.
    /// </summary>
    public int LoadedGrams { get; private set; }

    /// <summary>
    /// Статус волны.
    /// </summary>
    public WaveStatus Status { get; private set; }

    /// <summary>
    /// Токен оптимистичной блокировки для защиты от гонок при резервировании веса.
    /// </summary>
    public byte[] Version { get; private set; } = [];

    /// <summary>
    /// Создаёт запланированную волну.
    /// </summary>
    /// <param name="number">Сквозной номер.</param>
    /// <param name="direction">Направление.</param>
    /// <param name="shipDate">Дата отправки.</param>
    /// <param name="orderCutoffDate">Дедлайн приёма заказов.</param>
    /// <param name="capacityGrams">Вместимость в граммах.</param>
    /// <param name="minLoadGrams">Минимальная загрузка в граммах.</param>
    /// <returns>Новая волна.</returns>
    /// <exception cref="DomainException">Параметры волны некорректны.</exception>
    public static Wave Create(
        int number,
        ShippingDirection direction,
        DateOnly shipDate,
        DateOnly orderCutoffDate,
        int capacityGrams,
        int minLoadGrams)
    {
        if (number <= 0)
        {
            throw new DomainException("Номер волны должен быть положительным.");
        }

        if (capacityGrams <= 0)
        {
            throw new DomainException("Вместимость волны должна быть больше нуля.");
        }

        if (minLoadGrams <= 0 || minLoadGrams > capacityGrams)
        {
            throw new DomainException("Минимальная загрузка волны должна быть больше нуля и не больше вместимости.");
        }

        if (orderCutoffDate >= shipDate)
        {
            throw new DomainException("Приём заказов должен заканчиваться раньше даты отправки волны.");
        }

        return new Wave(number, direction, shipDate, orderCutoffDate, capacityGrams, minLoadGrams);
    }

    /// <summary>
    /// Проверяет, принимает ли волна заказы на указанный момент местного времени.
    /// </summary>
    /// <param name="localNow">Текущее местное время в часовом поясе региона.</param>
    /// <returns><see langword="true"/>, если волна открыта.</returns>
    public bool IsOpenForOrders(DateTime localNow) =>
        Status == WaveStatus.Planned && DateOnly.FromDateTime(localNow) <= OrderCutoffDate;

    /// <summary>
    /// Остаток времени до закрытия приёма заказов.
    /// </summary>
    /// <param name="localNow">Текущее местное время.</param>
    /// <returns>Остаток до конца дедлайна включительно.</returns>
    public TimeSpan TimeLeftUntilCutoff(DateTime localNow)
    {
        DateOnly today = DateOnly.FromDateTime(localNow);
        DateOnly cutoff = OrderCutoffDate;

        if (today > cutoff)
        {
            return TimeSpan.Zero;
        }

        DateOnly nextDay = today.AddDays(1);
        DateOnly endOfCutoffDay = cutoff == today ? nextDay : cutoff;

        return endOfCutoffDay.ToDateTime(TimeOnly.MinValue) - localNow;
    }

    /// <summary>
    /// Проверяет, помещается ли заказ в оставшуюся вместимость.
    /// </summary>
    /// <param name="orderGrams">Вес заказа в граммах.</param>
    /// <returns><see langword="true"/>, если заказ помещается.</returns>
    public bool CanAccept(int orderGrams) => orderGrams > 0 && LoadedGrams + orderGrams <= CapacityGrams;

    /// <summary>
    /// Заполнение волны в процентах.
    /// </summary>
    /// <param name="orderGrams">Вес добавляемого заказа.</param>
    /// <returns>Заполнение в процентах.</returns>
    public decimal GetFillPercent(int orderGrams) =>
        CapacityGrams == 0 ? 0m : (decimal)(LoadedGrams + orderGrams) * 100m / CapacityGrams;

    /// <summary>
    /// Свободная вместимость волны в граммах.
    /// </summary>
    /// <returns>Свободная вместимость.</returns>
    public int GetRemainingCapacity() => Math.Max(0, CapacityGrams - LoadedGrams);

    /// <summary>
    /// Проверяет готовность волны к отправке.
    /// </summary>
    /// <returns><see langword="true"/>, если набрана минимальная загрузка.</returns>
    public bool IsReadyToDispatch() => Status == WaveStatus.Planned && LoadedGrams >= MinLoadGrams;

    /// <summary>
    /// Резервирует вес заказа в волне.
    /// </summary>
    /// <param name="orderGrams">Вес заказа в граммах.</param>
    /// <exception cref="DomainException">Заказ не помещается в волну.</exception>
    public void Reserve(int orderGrams)
    {
        if (!CanAccept(orderGrams))
        {
            throw new DomainException(string.Create(
                CultureInfo.CurrentCulture,
                $"Волна №{Number} почти заполнена: свободно {GetRemainingCapacity()} г, а заказ весит {orderGrams} г."));
        }

        LoadedGrams += orderGrams;
    }

    /// <summary>
    /// Освобождает вес отменённого заказа.
    /// </summary>
    /// <param name="orderGrams">Вес заказа в граммах.</param>
    public void Release(int orderGrams) => LoadedGrams = Math.Max(0, LoadedGrams - orderGrams);

    /// <summary>
    /// Отправляет волну.
    /// </summary>
    /// <exception cref="DomainException">Волна уже отправлена или не набрала минимальную загрузку.</exception>
    public void MarkDispatched()
    {
        if (Status == WaveStatus.Dispatched)
        {
            throw new DomainException("Волна уже отправлена.");
        }

        if (LoadedGrams < MinLoadGrams)
        {
            throw new DomainException(string.Create(
                CultureInfo.CurrentCulture,
                $"Отправить волну нельзя: собрано {LoadedGrams / GramsPerKilogram:0.#} кг из требуемых {MinLoadGrams / GramsPerKilogram:0.#} кг."));
        }

        Status = WaveStatus.Dispatched;
    }
}