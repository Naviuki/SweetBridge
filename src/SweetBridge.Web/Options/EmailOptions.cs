using System.ComponentModel.DataAnnotations;

namespace SweetBridge.Web.Options;

/// <summary>
/// Параметры отправки писем. Режим <c>Outbox</c> пишет письма на диск вместо реальной отправки.
/// </summary>
public sealed class EmailOptions
{
    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// Режим работы: <c>Outbox</c> или <c>Smtp</c>.
    /// </summary>
    [Required]
    public string Mode { get; set; } = "Outbox";

    /// <summary>
    /// Папка для писем в режиме <c>Outbox</c>.
    /// </summary>
    [Required]
    public string OutboxPath { get; set; } = "App_Data/outbox";

    /// <summary>
    /// Адрес отправителя.
    /// </summary>
    [Required]
    [EmailAddress]
    public string FromAddress { get; set; } = "hello@sweetbridge.ru";

    /// <summary>
    /// Параметры SMTP-сервера.
    /// </summary>
    public SmtpOptions Smtp { get; set; } = new();
}

/// <summary>
/// Параметры SMTP-сервера. Секреты хранятся только в user-secrets или переменных окружения.
/// </summary>
public sealed class SmtpOptions
{
    /// <summary>
    /// Имя сервера.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Порт.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    /// <summary>
    /// Логин.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Пароль.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}