namespace SweetBridge.Web.Infrastructure.Security;

/// <summary>
/// Имена политик ограничения частоты запросов.
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>
    /// Вход, регистрация и сброс пароля.
    /// </summary>
    public const string Auth = "Auth";

    /// <summary>
    /// Отслеживание заказа.
    /// </summary>
    public const string Tracking = "Tracking";

    /// <summary>
    /// Применение промокода.
    /// </summary>
    public const string Promo = "Promo";

    /// <summary>
    /// Формы поддержки, подписки и корпоративных заявок.
    /// </summary>
    public const string Forms = "Forms";

    /// <summary>
    /// Общий лимит на все запросы.
    /// </summary>
    public const string Global = "Global";

    /// <summary>
    /// Имя параметра раздела конфигурации с лимитами.
    /// </summary>
    public const string OptionsSection = "RateLimits";
}