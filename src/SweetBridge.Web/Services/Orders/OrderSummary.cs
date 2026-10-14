using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Orders;

/// <summary>
/// Полная модель чтения заказа.
/// </summary>
public sealed record OrderSummary(
    int Id,
    string Number,
    string? UserId,
    DateTime CreatedAtUtc,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    bool IsGift,
    string BuyerName,
    string BuyerPhone,
    string RecipientName,
    string RecipientPhone,
    string RecipientCity,
    string? RecipientPickupPoint,
    string? GiftMessage,
    int WaveId,
    int WaveNumber,
    DateOnly WaveShipDate,
    int TotalWeightGrams,
    decimal GoodsCost,
    decimal DiscountAmount,
    string? PromoCode,
    decimal ShippingCost,
    decimal ThermoPackCost,
    decimal FillDiscountPercent,
    decimal Total,
    DateTime? PaymentDueAtUtc,
    IReadOnlyList<OrderLineSummary> Items,
    IReadOnlyList<OrderStatusPoint> History)
{
    /// <summary>
    /// Признак того, что заказ принадлежит пользователю.
    /// </summary>
    /// <param name="userId">Идентификатор текущего пользователя.</param>
    /// <returns><see langword="true"/>, если заказ найден в кабинете пользователя.</returns>
    public bool BelongsTo(string? userId) =>
        !string.IsNullOrEmpty(UserId) && string.Equals(UserId, userId, StringComparison.Ordinal);

    /// <summary>
    /// Подписи шагов трекера до текущего статуса.
    /// </summary>
    /// <returns>Подписи шагов.</returns>
    public OrderTrackerViewModel ToTracker()
    {
        string[] steps = ["Принят", "Собирается", "В пути", "Готов к выдаче", "Получен"];
        int current = Status == OrderStatus.Cancelled ? 0 : (int)Status;

        return new OrderTrackerViewModel(steps, Math.Clamp(current, 0, steps.Length - 1));
    }
}