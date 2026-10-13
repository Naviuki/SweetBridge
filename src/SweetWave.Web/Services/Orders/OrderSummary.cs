using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Read model of an order for the confirmation, tracking and admin pages.
/// </summary>
/// <param name="Id">Order identifier.</param>
/// <param name="Number">Order number.</param>
/// <param name="CreatedAtUtc">Creation moment, in UTC.</param>
/// <param name="Status">Current status.</param>
/// <param name="IsGift">Indicates that the order is a gift.</param>
/// <param name="BuyerName">Buyer name.</param>
/// <param name="BuyerPhone">Buyer phone number.</param>
/// <param name="RecipientName">Recipient name.</param>
/// <param name="RecipientPhone">Recipient phone number.</param>
/// <param name="RecipientCity">Recipient city.</param>
/// <param name="RecipientPickupPoint">Pickup point.</param>
/// <param name="GiftMessage">Greeting card text.</param>
/// <param name="WaveId">Wave identifier.</param>
/// <param name="WaveShipDate">Shipment date of the wave.</param>
/// <param name="TotalWeightGrams">Total weight of the order, in grams.</param>
/// <param name="GoodsCost">Cost of the goods, in rubles.</param>
/// <param name="ShippingCost">Delivery cost, in rubles.</param>
/// <param name="ThermoPackCost">Thermal packaging cost, in rubles.</param>
/// <param name="AppliedDiscountPercent">Applied delivery discount, in percent.</param>
/// <param name="Items">Order positions.</param>
/// <param name="History">Status timeline.</param>
public sealed record OrderSummary(
    int Id,
    string Number,
    DateTime CreatedAtUtc,
    OrderStatus Status,
    bool IsGift,
    string BuyerName,
    string BuyerPhone,
    string RecipientName,
    string RecipientPhone,
    string RecipientCity,
    string RecipientPickupPoint,
    string? GiftMessage,
    int WaveId,
    DateOnly WaveShipDate,
    int TotalWeightGrams,
    decimal GoodsCost,
    decimal ShippingCost,
    decimal ThermoPackCost,
    decimal AppliedDiscountPercent,
    IReadOnlyList<OrderLineSummary> Items,
    IReadOnlyList<OrderStatusPoint> History)
{
    /// <summary>
    /// Total amount payable, in rubles.
    /// </summary>
    public decimal TotalCost => GoodsCost + ShippingCost + ThermoPackCost;

    /// <summary>
    /// Indicates that thermal packaging was paid for.
    /// </summary>
    public bool HasThermoPack => ThermoPackCost > 0m;
}