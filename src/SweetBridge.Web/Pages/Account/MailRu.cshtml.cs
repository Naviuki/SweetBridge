using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Options;

namespace SweetBridge.Web.Pages.Account;

/// <summary>
/// Вход через Mail.ru: авторизация на стороне Mail.ru и обмен кода на токен.
/// Сделано собственными обработчиками, чтобы поведение не зависело от версии OAuth-обработчиков.
/// </summary>
/// <param name="userManager">Менеджер пользователей Identity.</param>
/// <param name="signInManager">Менеджер входа Identity.</param>
/// <param name="externalAuth">Параметры внешней авторизации.</param>
[AllowAnonymous]
public sealed class MailRuModel(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IOptions<ExternalAuthOptions> externalAuth)
    : PageModel
{
    private const string AuthorizationUrl = "https://oauth.mail.ru/login";
    private const string TokenUrl = "https://oauth.mail.ru/token";
    private const string UserInfoUrl = "https://oauth.mail.ru/userinfo?format=json";
    private const string StateKey = "SweetBridge.MailRu.State";

    private readonly ExternalAuthProvider _provider = externalAuth.Value.MailRu;

    /// <summary>
    /// Признак того, что вход через Mail.ru настроен.
    /// </summary>
    [BindNever] // Служебное свойство страницы: не участвует в биндинге модели.
    public bool IsConfigured => _provider.IsConfigured;

    /// <summary>
    /// Отправляет покупателя на страницу авторизации Mail.ru.
    /// </summary>
    /// <returns>Перенаправление на Mail.ru.</returns>
    public IActionResult OnGet()
    {
        if (!IsConfigured)
        {
            TempData["Error"] = "Вход через Mail.ru не настроен: задайте ExternalAuth:MailRu:ClientId и ClientSecret.";

            return RedirectToPage("./Login");
        }

        string state = RandomNumberGenerator.GetHexString(32);
        HttpContext.Session.SetString(StateKey, state);

        string challenge = AuthorizationUrl + "?" + string.Join(
            "&",
            "response_type=code",
            "client_id=" + Uri.EscapeDataString(_provider.ClientId),
            "redirect_uri=" + Uri.EscapeDataString(BuildCallbackUrl()),
            "scope=" + Uri.EscapeDataString("userinfo"),
            "state=" + Uri.EscapeDataString(state),
            "lang=ru-RU");

        return Redirect(challenge);
    }

    /// <summary>
    /// Обрабатывает ответ Mail.ru: обмен кода на токен, профиль и вход.
    /// </summary>
    /// <param name="code">Код авторизации.</param>
    /// <param name="error">Ошибка от Mail.ru.</param>
    /// <returns>Перенаправление в личный кабинет.</returns>
    public async Task<IActionResult> OnGetCallbackAsync(string? code, string? error)
    {
        string? expectedState = HttpContext.Session.GetString(StateKey);
        HttpContext.Session.Remove(StateKey);

        if (!string.IsNullOrEmpty(error))
        {
            TempData["Error"] = "Вход через Mail.ru отменён.";

            return RedirectToPage("./Login");
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrEmpty(expectedState))
        {
            TempData["Error"] = "Не удалось подтвердить вход через Mail.ru. Попробуйте ещё раз.";

            return RedirectToPage("./Login");
        }

        string token = await ExchangeCodeAsync(code);

        if (token.Length == 0)
        {
            TempData["Error"] = "Mail.ru не выдал токен доступа. Попробуйте войти по почте.";

            return RedirectToPage("./Login");
        }

        AppUser user = await FindOrCreateAsync(await ReadProfileAsync(token));

        await signInManager.SignInAsync(user, isPersistent: true);

        return RedirectToPage("./Orders");
    }

    private string BuildCallbackUrl()
    {
        string path = Url.Page("/Account/MailRu", "Callback") ?? "/Account/MailRu?handler=Callback";

        return string.Concat(Request.Scheme, "://", Request.Host.Value, path);
    }

    private async Task<string> ExchangeCodeAsync(string code)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        using var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = _provider.ClientId,
                ["client_secret"] = _provider.ClientSecret,
                ["code"] = code,
                ["redirect_uri"] = BuildCallbackUrl()
            })
        };

        using HttpResponseMessage response = await client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return string.Empty;
        }

        using JsonDocument document = JsonDocument.Parse(body);

        return document.RootElement.TryGetProperty("access_token", out JsonElement token)
            ? token.GetString() ?? string.Empty
            : string.Empty;
    }

    private static async Task<string> ReadProfileAsync(string token)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await client.GetStringAsync(UserInfoUrl);
    }

    private async Task<AppUser> FindOrCreateAsync(string profileJson)
    {
        using JsonDocument document = JsonDocument.Parse(profileJson);
        JsonElement root = document.RootElement;

        string email = ReadString(root, "email")
            ?? throw new InvalidOperationException("Mail.ru не вернул адрес электронной почты.");

        AppUser? user = await userManager.FindByEmailAsync(email);

        if (user is not null)
        {
            return user;
        }

        user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = (ReadString(root, "name") ?? ReadString(root, "nickname") ?? email).Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        IdentityResult created = await userManager.CreateAsync(user);

        if (!created.Succeeded)
        {
            throw new InvalidOperationException(
                "Не удалось создать аккаунт: "
                + string.Join("; ", created.Errors.Select(item => item.Description)));
        }

        return user;
    }

    private static string? ReadString(JsonElement root, string name) =>
        root.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
