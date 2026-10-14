namespace SweetBridge.Web.Options;

/// <summary>
/// Параметры начального наполнения базы данных.
/// </summary>
public sealed class SeedOptions
{
    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public const string SectionName = "Seed";

    /// <summary>
    /// Почта администратора, создаётся при первом запуске.
    /// </summary>
    public string AdminEmail { get; set; } = "admin@sweetbridge.local";

    /// <summary>
    /// Пароль администратора. Пустое значение означает, что пароль приходит из секретов.
    /// </summary>
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>
    /// Создавать ли демонстрационные данные: товары, волны, промокоды и заказы.
    /// </summary>
    public bool CreateDemoData { get; set; } = true;
}