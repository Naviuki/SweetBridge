using System.ComponentModel.DataAnnotations;

namespace SweetBridge.Web.Options;

/// <summary>
/// Лимиты частоты запросов по секциям конфигурации <c>RateLimits</c>.
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public const string SectionName = "RateLimits";

    /// <summary>
    /// Вход, регистрация и сброс пароля.
    /// </summary>
    [Required]
    public RateLimitPolicyOptions Auth { get; set; } = new() { Permits = 5, WindowSeconds = 60 };

    /// <summary>
    /// Отслеживание заказа.
    /// </summary>
    [Required]
    public RateLimitPolicyOptions Tracking { get; set; } = new() { Permits = 10, WindowSeconds = 60 };

    /// <summary>
    /// Применение промокода.
    /// </summary>
    [Required]
    public RateLimitPolicyOptions Promo { get; set; } = new() { Permits = 10, WindowSeconds = 60 };

    /// <summary>
    /// Формы поддержки, подписки и корпоративных заявок.
    /// </summary>
    [Required]
    public RateLimitPolicyOptions Forms { get; set; } = new() { Permits = 5, WindowSeconds = 60 };

    /// <summary>
    /// Общий лимит на все запросы.
    /// </summary>
    [Required]
    public RateLimitPolicyOptions Global { get; set; } = new() { Permits = 300, WindowSeconds = 60 };
}

/// <summary>
/// Одна политика ограничения частоты.
/// </summary>
public sealed class RateLimitPolicyOptions
{
    /// <summary>
    /// Количество запросов в окне.
    /// </summary>
    [Range(1, 100_000)]
    public int Permits { get; set; }

    /// <summary>
    /// Длина окна в секундах.
    /// </summary>
    [Range(1, 3600)]
    public int WindowSeconds { get; set; }
}