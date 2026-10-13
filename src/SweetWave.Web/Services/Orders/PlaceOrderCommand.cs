namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Checkout form data. Prices and weights are always recalculated on the server.
/// </summary>
/// <param name="WaveId">Chosen wave.</param>
/// <param name="BuyerName">Buyer name.</param>
/// <param name="BuyerPhone">Buyer phone number.</param>
/// <param name="IsGift">Indicates that the order is a gift.</param>
/// <param name="RecipientName">Recipient name for a gift.</param>
/// <param name="RecipientPhone">Recipient phone number for a gift.</param>
/// <param name="RecipientCity">Recipient city.</param>
/// <param name="RecipientPickupPoint">Pickup point.</param>
/// <param name="GiftMessage">Greeting card text.</param>
/// <param name="ThermoPackRequested">Thermal packaging chosen by the buyer.</param>
/// <param name="Lines">Ordered positions.</param>
public sealed record PlaceOrderCommand(
    int WaveId,
    string BuyerName,
    string BuyerPhone,
    bool IsGift,
    string? RecipientName,
    string? RecipientPhone,
    string? RecipientCity,
    string? RecipientPickupPoint,
    string? GiftMessage,
    bool ThermoPackRequested,
    IReadOnlyList<OrderLineRequest> Lines);