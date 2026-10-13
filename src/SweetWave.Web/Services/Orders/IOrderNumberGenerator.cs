namespace SweetWave.Web.Services.Orders;

/// <summary>
/// Generates unique order numbers in the <c>SW-XXXXXX</c> format.
/// </summary>
public interface IOrderNumberGenerator
{
    /// <summary>
    /// Generates an order number that is not used yet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Unique order number.</returns>
    Task<string> GenerateAsync(CancellationToken cancellationToken);
}