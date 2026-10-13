using System.ComponentModel.DataAnnotations;

namespace SweetWave.Web.Options;

/// <summary>
/// Учётные данные администратора, используемые в админке.
/// </summary>
public sealed class AdminOptions
{
    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public const string SectionName = "Admin";

    /// <summary>
    /// Логин администратора.
    /// </summary>
    [Required]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Пароль администратора.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}
