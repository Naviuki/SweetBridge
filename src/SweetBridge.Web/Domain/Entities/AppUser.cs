using Microsoft.AspNetCore.Identity;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Пользователь платформы: покупатель или администратор.
/// </summary>
public sealed class AppUser : IdentityUser
{
    /// <summary>
    /// Отображаемое имя.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Нормализованный телефон в формате <c>+7XXXXXXXXXX</c>.
    /// </summary>
    public string? PhoneNumberNormalized { get; set; }

    /// <summary>
    /// Момент регистрации, в UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Признак блокировки, выставленный администратором.
    /// </summary>
    public bool IsBlocked { get; set; }
}