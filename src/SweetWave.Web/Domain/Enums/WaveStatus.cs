namespace SweetWave.Web.Domain.Enums;

/// <summary>
/// State of a consolidated delivery wave.
/// </summary>
public enum WaveStatus
{
    /// <summary>
    /// Wave accepts orders.
    /// </summary>
    Planned = 0,

    /// <summary>
    /// Wave has been dispatched and no longer accepts orders.
    /// </summary>
    Dispatched = 1
}