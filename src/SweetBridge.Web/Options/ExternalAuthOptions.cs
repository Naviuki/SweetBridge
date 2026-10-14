namespace SweetBridge.Web.Options;

/// <summary>
/// Параметры внешних провайдеров авторизации.
/// </summary>
public sealed class ExternalAuthOptions
{
    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public const string SectionName = "ExternalAuth";

    /// <summary>
    /// Параметры Google.
    /// </summary>
    public ExternalAuthProvider Google { get; set; } = new();

    /// <summary>
    /// Параметры VK.
    /// </summary>
    public ExternalAuthProvider Vk { get; set; } = new();
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