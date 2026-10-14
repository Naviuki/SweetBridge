namespace SweetBridge.Web.Domain.Enums;

/// <summary>
/// Статус обращения в поддержку.
/// </summary>
public enum SupportRequestStatus
{
    /// <summary>
    /// Новое обращение.
    /// </summary>
    New = 0,

    /// <summary>
    /// В работе.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Закрыто.
    /// </summary>
    Closed = 2
}