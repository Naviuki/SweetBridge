using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Order lifecycle: checkout, tracking and administration.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Places an order into a wave: prices, weights and capacity are recalculated on the server.
    /// </summary>
    /// <param name="command">Checkout data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created order.</returns>
    Task<OrderSummary> PlaceAsync(PlaceOrderCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Finds an order by its number and a phone number of the buyer or the recipient.
    /// </summary>
    /// <param name="number">Order number.</param>
    /// <param name="phone">Phone number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Order or <see langword="null"/> when nothing is found.</returns>
    Task<OrderSummary?> FindAsync(string number, string phone, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the orders that match the filter for the admin list.
    /// </summary>
    /// <param name="filter">Status and wave filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching orders.</returns>
    Task<IReadOnlyList<OrderSummary>> GetOrdersAsync(OrderFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Moves the order to the next status of the lifecycle.
    /// </summary>
    /// <param name="orderId">Order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated order.</returns>
    Task<OrderSummary> AdvanceStatusAsync(int orderId, CancellationToken cancellationToken);
}