using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SweetBridge.Tests.Integration;

/// <summary>
/// Сквозные сценарии: регистрация, вход, корзина из каталога, оформление и оплата заказа.
/// Тесты ловят регрессии вроде потерянного биндинга модели или падения редиректа.
/// </summary>
public sealed class AccountFlowTests(SiteWebApplicationFactory factory) : IClassFixture<SiteWebApplicationFactory>
{
    private readonly SiteWebApplicationFactory _factory = factory;

    /// <summary>
    /// Регистрация с корректными данными создаёт аккаунт и открывает личный кабинет.
    /// </summary>
    /// <returns>Задача проверки.</returns>
    [Fact]
    public async Task Register_WithValidData_CreatesAccountAndSignsIn()
    {
        HttpClient client = CreateClient();

        string form = await GetHtmlAsync(client, "/Account/Register");
        string email = NewEmail();

        using HttpResponseMessage response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(Form(
            Token(form),
            ("Input.Name", "Тестовый Покупатель"),
            ("Input.Email", email),
            ("Input.Phone", "+7 999 123-45-67"),
            ("Input.Password", "Passw0rd!2026"),
            ("Input.ConfirmPassword", "Passw0rd!2026"))));

        Assert.True(
            response.StatusCode == HttpStatusCode.Found,
            $"Ожидался переход в личный кабинет, получен {(int)response.StatusCode}. Ошибки: {Errors(Decode(response))}");

        using HttpResponseMessage orders = await client.GetAsync("/Account/Orders");
        Assert.Equal(HttpStatusCode.OK, orders.StatusCode);
    }

    /// <summary>
    /// Вход по созданному аккаунту открывает личный кабинет.
    /// </summary>
    /// <returns>Задача проверки.</returns>
    [Fact]
    public async Task Login_WithValidCredentials_OpensAccount()
    {
        HttpClient client = CreateClient();
        string email = NewEmail();

        await RegisterAsync(client, email);

        HttpClient fresh = CreateClient();
        string form = await GetHtmlAsync(fresh, "/Account/Login");

        using HttpResponseMessage response = await fresh.PostAsync("/Account/Login", new FormUrlEncodedContent(Form(
            Token(form),
            ("Input.Email", email),
            ("Input.Password", "Passw0rd!2026"),
            ("Input.RememberMe", "false"))));

        Assert.True(
            response.StatusCode == HttpStatusCode.Found,
            $"Ожидался переход после входа, получен {(int)response.StatusCode}. Ошибки: {Errors(Decode(response))}");
    }

    /// <summary>
    /// Добавление товара в корзину из каталога возвращает в каталог с теми же фильтрами.
    /// </summary>
    /// <returns>Задача проверки.</returns>
    [Fact]
    public async Task AddToCart_FromCatalog_KeepsFiltersAndRedirects()
    {
        HttpClient client = CreateClient();
        string catalog = await GetHtmlAsync(client, "/Catalog?direction=DonbassToYakutia");

        string productId = Regex.Match(catalog, "data-product-id=\"(?<id>[^\"]+)\"").Groups["id"].Value;
        Assert.False(string.IsNullOrWhiteSpace(productId), "В каталоге должен быть хотя бы один товар");

        using HttpResponseMessage response = await client.PostAsync(
            "/Catalog?handler=AddToCart&direction=DonbassToYakutia",
            new FormUrlEncodedContent(Form(Token(catalog), ("productId", productId), ("quantity", "2"))));

        Assert.True(
            response.StatusCode == HttpStatusCode.Found,
            $"Ожидался возврат в каталог, получен {(int)response.StatusCode}");

        string location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("direction=DonbassToYakutia", location, StringComparison.Ordinal);
        Assert.DoesNotContain("page=2", location, StringComparison.Ordinal);

        string cart = await GetHtmlAsync(client, "/Cart");
        Assert.Contains(productId, cart, StringComparison.Ordinal);
    }

    private HttpClient CreateClient() => _factory.CreateClient(
        new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static async Task RegisterAsync(HttpClient client, string email)
    {
        string form = await GetHtmlAsync(client, "/Account/Register");

        using HttpResponseMessage response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(Form(
            Token(form),
            ("Input.Name", "Тестовый Покупатель"),
            ("Input.Email", email),
            ("Input.Phone", "+7 999 123-45-67"),
            ("Input.Password", "Passw0rd!2026"),
            ("Input.ConfirmPassword", "Passw0rd!2026"))));

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }

    private static string NewEmail() => $"flow{Guid.NewGuid():N}@test.local";

    private static List<KeyValuePair<string, string>> Form(string token, params (string Key, string Value)[] fields) =>
    [
        new("__RequestVerificationToken", token),
        .. fields.Select(field => new KeyValuePair<string, string>(field.Key, field.Value))
    ];

    private static string Decode(HttpResponseMessage response) =>
        WebUtility.HtmlDecode(response.Content.ReadAsStringAsync().Result);

    private static string Errors(string html) => string.Join(
        " / ",
        Regex.Matches(html, "<span class=\"field__error\"[^>]*>([^<]+)</span>")
            .Select(match => match.Groups[1].Value)
            .Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string Token(string html) => Regex
        .Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"(?<token>[^\"]+)\"")
        .Groups["token"].Value;

    private static async Task<string> GetHtmlAsync(HttpClient client, string url)
    {
        using HttpResponseMessage response = await client.GetAsync(url);

        return Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());
    }
}