namespace SweetWave.Web.Services.Delivery;

/// <summary>
/// Calculates the cost of a consolidated delivery.
/// </summary>
public interface IDeliveryCostCalculator
{
    /// <summary>
    /// Calculates the delivery cost for an order.
    /// </summary>
    /// <param name="request">Weight, wave fill and thermal packaging requirement.</param>
    /// <returns>Calculated costs.</returns>
    DeliveryCostResult Calculate(DeliveryCostRequest request);
}