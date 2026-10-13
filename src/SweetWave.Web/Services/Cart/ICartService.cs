namespace SweetWave.Web.Services.Cart;

/// <summary>
/// Shopping cart kept in the session.
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Returns the number of goods in the cart without accessing the database.
    /// </summary>
    /// <returns>Total quantity of the cart.</returns>
    int GetItemCount();

    /// <summary>
    /// Returns the cart with the actual prices and weights.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current cart.</returns>
    Task<CartView> GetCartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds a product to the cart.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="quantity">Quantity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated cart.</returns>
    Task<CartView> AddItemAsync(string productId, int quantity, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the quantity of a line: a quantity below one removes the line.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="quantity">New quantity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated cart.</returns>
    Task<CartView> SetQuantityAsync(string productId, int quantity, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a line from the cart.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated cart.</returns>
    Task<CartView> RemoveItemAsync(string productId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes all lines from the cart.
    /// </summary>
    void Clear();
}