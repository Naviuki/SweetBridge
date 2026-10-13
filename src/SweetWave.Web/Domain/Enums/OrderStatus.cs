namespace SweetWave.Web.Domain.Enums;

/// <summary>
/// Order lifecycle. Status can only move forward, one step at a time.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order is accepted and waiting for the wave to be assembled.
    /// </summary>
    Accepted = 0,

    /// <summary>
    /// Order is being packed into the wave.
    /// </summary>
    Assembling = 1,

    /// <summary>
    /// Wave is on the way.
    /// </summary>
    InTransit = 2,

    /// <summary>
    /// Order arrived at the pickup point.
    /// </summary>
    ReadyForPickup = 3,

    /// <summary>
    /// Order is received by the buyer or the gift recipient.
    /// </summary>
    Received = 4
}