using System.ComponentModel.DataAnnotations;

namespace SweetBridge.Web.Options;

/// <summary>
/// Контактные данные компании для подвала и страницы контактов.
/// </summary>
public sealed class ContactsOptions
{
    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public const string SectionName = "Contacts";

    /// <summary>
    /// Почта для связи.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Телефон для связи.
    /// </summary>
    [Required]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Адрес офиса.
    /// </summary>
    [Required]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Часы работы.
    /// </summary>
    [Required]
    public string Hours { get; set; } = string.Empty;
}