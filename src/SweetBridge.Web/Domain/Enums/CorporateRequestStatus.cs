namespace SweetBridge.Web.Domain.Enums;

/// <summary>
/// Статус корпоративной заявки.
/// </summary>
public enum CorporateRequestStatus
{
    /// <summary>
    /// Новая заявка.
    /// </summary>
    New = 0,

    /// <summary>
    /// Согласование условий.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Закрыта.
    /// </summary>
    Closed = 2
}