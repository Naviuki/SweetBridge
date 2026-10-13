namespace SweetWave.Web.Services.Cart;

/// <summary>
/// Cart line stored in the session: only the identifier and the quantity are kept,
/// prices and weights are always read from the database.
/// </summary>
/// <param name="ProductId">Product identifier.</param>
/// <param name="Quantity">Ordered quantity.</param>
public sealed record CartLineState(string ProductId, int Quantity);