namespace SweetWave.Web.Services.Cart;

/// <summary>
/// Позиция корзины, хранящаяся в сессии: сохраняются только идентификатор и количество,
/// а цены и веса всегда читаются из базы данных.
/// </summary>
/// <param name="productId">Идентификатор товара.</param>
/// <param name="quantity">Заказанное количество.</param>
public sealed record CartLineState(string ProductId, int Quantity);
