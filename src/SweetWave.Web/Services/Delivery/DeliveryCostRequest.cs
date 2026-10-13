namespace SweetWave.Web.Services.Delivery;

/// <summary>
/// Input of the delivery cost calculation.
/// </summary>
/// <param name="WeightGrams">Weight of the order, in grams.</param>
/// <param name="WaveFillPercent">Fill percentage of the wave after adding the order, in percent.</param>
/// <param name="ThermoPackRequired">Indicates that thermal packaging has to be paid for.</param>
public sealed record DeliveryCostRequest(int WeightGrams, decimal WaveFillPercent, bool ThermoPackRequired);