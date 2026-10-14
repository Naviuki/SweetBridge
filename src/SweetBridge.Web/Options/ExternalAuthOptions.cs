namespace SweetBridge.Web.Options;

/// <summary>
/// Параметры внешней авторизации.
/// </summary>
public sealed class ExternalAuthOptions
{
    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public const string SectionName = "ExternalAuth";

    /// <summary>
    /// Параметры Mail.ru.
    /// </summary>
    public ExternalAuthProvider MailRu { get; set; } = new();
}

/// <summary>
/// Идентификатор и секрет внешнего провайдера авторизации.
/// </summary>
public sealed class ExternalAuthProvider
{
    /// <summary>
    /// Идентификатор приложения.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Секрет приложения.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Заданы ли данные провайдера.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}