namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Single position of an order requested by the buyer.
/// </summary>
/// <param name="ProductId">Product identifier.</param>
/// <param name="Quantity">Requested quantity.</param>
public sealed record OrderLineRequest(string ProductId, int Quantity);