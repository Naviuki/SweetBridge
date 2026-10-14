using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace SweetBridge.Web.Services.Account;

/// <summary>
/// Правила пароля из настроек Identity в понятном покупателю виде.
/// Текст подсказки берётся из тех же настроек, что проверяет сервер,
/// поэтому подсказка и реальные правила не могут разойтись.
/// </summary>
/// <param name="options">Параметры паролей Identity.</param>
public sealed class PasswordPolicyText(IOptions<IdentityOptions> options)
{
    private readonly IdentityOptions _options = options.Value;

    /// <summary>
    /// Текст подсказки, например «Минимум 8 символов, цифра, заглавная буква и спецсимвол».
    /// </summary>
    public string Hint
    {
        get
        {
            var parts = new List<string> { $"Минимум {_options.Password.RequiredLength} символов" };

            if (_options.Password.RequireDigit)
            {
                parts.Add("цифра");
            }

            if (_options.Password.RequireLowercase)
            {
                parts.Add("строчная буква");
            }

            if (_options.Password.RequireUppercase)
            {
                parts.Add("заглавная буква");
            }

            if (_options.Password.RequireNonAlphanumeric)
            {
                parts.Add("спецсимвол");
            }

            return string.Join(", ", parts);
        }
    }

    /// <summary>
    /// Текст требований для администратора.
    /// </summary>
    public string Requirements => $"{Hint}. Требования задаются в параметрах Identity.";
}