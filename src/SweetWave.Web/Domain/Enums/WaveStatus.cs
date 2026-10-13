namespace SweetWave.Web.Domain.Enums;

/// <summary>
/// Состояние сборной волны доставки.
/// </summary>
public enum WaveStatus
{
    /// <summary>
    /// Волна принимает заказы.
    /// </summary>
    Planned = 0,

    /// <summary>
    /// Волна отправлена и больше не принимает заказы.
    /// </summary>
    Dispatched = 1
}
