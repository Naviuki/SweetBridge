namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Одна позиция заказа, запрошенная покупателем.
/// </summary>
/// <param name="productId">Идентификатор товара.</param>
/// <param name="Quantity">Запрошенное количество.</param>
public sealed record OrderLineRequest(string ProductId, int Quantity);
