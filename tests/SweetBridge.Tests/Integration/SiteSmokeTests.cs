using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SweetBridge.Tests.Integration;

/// <summary>
/// Интеграционные проверки публичного интерфейса: фильтры каталога, подгрузка товаров,
/// иконки футера, подсказка пароля, отсутствие упоминаний хакатона и строгая CSP.
/// </summary>
public sealed class SiteSmokeTests : IClassFixture<SiteWebApplicationFactory>
{
    private readonly SiteWebApplicationFactory _factory;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="SiteSmokeTests"/>.
    /// </summary>
    /// <param name="factory">Фабрика тестового сервера.</param>
    public SiteSmokeTests(SiteWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Catalog_WithFilters_RendersThemInForm()
    {
        HttpClient client = _factory.CreateClient();

        string html = await GetHtmlAsync(client, "/Catalog?direction=DonbassToYakutia&q=%D1%88%D0%BE%D0%BA&thermo=true");

        Assert.Contains("name=\"thermo\" value=\"true\" checked", html, StringComparison.Ordinal);
        Assert.Contains("name=\"direction\" value=\"DonbassToYakutia\"", html, StringComparison.Ordinal);
        Assert.True(
            Regex.IsMatch(html, "name=\"q\"\\s+value=\"шок\""),
            "Поле поиска должно сохранять введённый запрос после применения фильтра");
    }

    [Fact]
    public async Task Catalog_ShowsCountAndLoadMoreLink()
    {
        HttpClient client = _factory.CreateClient();

        string html = await GetHtmlAsync(client, "/Catalog?direction=DonbassToYakutia");

        Assert.Contains("data-products-counter", html, StringComparison.Ordinal);
        Assert.Contains("Показано", html, StringComparison.Ordinal);
        Assert.Contains("data-load-more", html, StringComparison.Ordinal);
        Assert.Contains("handler=Items", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Catalog_ItemsHandler_ReturnsNextPortionWithoutDuplicates()
    {
        HttpClient client = _factory.CreateClient();

        string first = await GetHtmlAsync(client, "/Catalog?direction=DonbassToYakutia");
        string second = await GetHtmlAsync(client, "/Catalog?handler=Items&direction=DonbassToYakutia&page=2");

        string[] firstIds = ProductIds(first);
        string[] secondIds = ProductIds(second);

        Assert.NotEmpty(firstIds);
        Assert.Empty(secondIds.Intersect(firstIds, StringComparer.Ordinal));
    }

    [Fact]
    public async Task Catalog_FilterLinks_KeepOtherFilters()
    {
        HttpClient client = _factory.CreateClient();

        string html = await GetHtmlAsync(client, "/Catalog?direction=DonbassToYakutia&q=%D1%88%D0%BE%D0%BA&thermo=true");

        Assert.Contains("q=%D1%88%D0%BE%D0%BA", html, StringComparison.Ordinal);
        Assert.Contains("thermo=true", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Footer_SocialCircles_UseInlineSpriteSymbols()
    {
        HttpClient client = _factory.CreateClient();

        string html = await GetHtmlAsync(client, "/");

        Assert.Contains("footer-social__link", html, StringComparison.Ordinal);
        Assert.Contains("use href=\"#icon-telegram\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("/icons/sprite.svg#", html, StringComparison.Ordinal);

        foreach (string id in Regex.Matches(html, "use href=\"#(?<id>[a-z-]+)\"")
                     .Select(match => match.Groups["id"].Value)
                     .Distinct())
        {
            Assert.Contains($"id=\"{id}\"", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Pages_DoNotMentionHackathon()
    {
        HttpClient client = _factory.CreateClient();

        foreach (string url in new[] { "/", "/Catalog", "/Gifts" })
        {
            string html = await GetHtmlAsync(client, url);

            Assert.DoesNotContain("PEAK", html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("хакатон", html, StringComparison.OrdinalIgnoreCase);
        }

        using HttpResponseMessage removed = await client.GetAsync("/Info/Hackathon");
        Assert.Equal(HttpStatusCode.NotFound, removed.StatusCode);
    }

    [Fact]
    public async Task HowItWorks_PageExists()
    {
        HttpClient client = _factory.CreateClient();

        string html = await GetHtmlAsync(client, "/Info/HowItWorks");

        Assert.Contains("Как работает", html, StringComparison.Ordinal);
        Assert.Contains("В каталог", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Register_PasswordHintMatchesIdentityOptions()
    {
        HttpClient client = _factory.CreateClient();

        string html = await GetHtmlAsync(client, "/Account/Register");

        using IServiceScope scope = _factory.Services.CreateScope();
        IdentityOptions options = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value;

        Assert.Contains($"Минимум {options.Password.RequiredLength} символов", html, StringComparison.Ordinal);

        if (options.Password.RequireDigit)
        {
            Assert.Contains("цифра", html, StringComparison.Ordinal);
        }

        if (options.Password.RequireUppercase)
        {
            Assert.Contains("заглавная буква", html, StringComparison.Ordinal);
        }

        if (options.Password.RequireNonAlphanumeric)
        {
            Assert.Contains("спецсимвол", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Register_PasswordReveal_ControlsBothFields()
    {
        HttpClient client = _factory.CreateClient();

        string html = await GetHtmlAsync(client, "/Account/Register");

        Assert.Contains("data-password-reveal", html, StringComparison.Ordinal);
        Assert.Contains("aria-controls=\"Input_Password Input_ConfirmPassword\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Fonts_AreServedLocally()
    {
        HttpClient client = _factory.CreateClient();

        string css = await GetHtmlAsync(client, "/css/fonts.css");

        Assert.Contains("Unbounded", css, StringComparison.Ordinal);
        Assert.Contains("Manrope", css, StringComparison.Ordinal);
        Assert.Contains("font-display: swap", css, StringComparison.Ordinal);

        using HttpResponseMessage font = await client.GetAsync("/fonts/unbounded-cyrillic-800-normal.woff2");
        Assert.Equal(HttpStatusCode.OK, font.StatusCode);
    }

    [Fact]
    public async Task Pages_HaveNoInlineHandlersOrStyles()
    {
        HttpClient client = _factory.CreateClient();

        foreach (string url in new[] { "/", "/Catalog?direction=DonbassToYakutia", "/Cart/Index", "/Account/Login" })
        {
            string html = await GetHtmlAsync(client, url);

            Assert.DoesNotContain("onclick=", html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("style=\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("href=\"#\"", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Header_Menu_And_Footer_ArePresentOnShortPages()
    {
        HttpClient client = _factory.CreateClient();

        foreach (string url in new[] { "/Account/Login", "/Orders/Track", "/Cart/Index" })
        {
            string html = await GetHtmlAsync(client, url);

            Assert.Contains("site-header", html, StringComparison.Ordinal);
            Assert.Contains("site-footer", html, StringComparison.Ordinal);
            Assert.Contains("/Info/HowItWorks", html, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Читает страницу как UTF-8 и раскрывает HTML-сущности:
    /// кодировщик Razor выводит кириллицу из выражений как &#xNNN;.
    /// </summary>
    /// <param name="client">Клиент тестового сервера.</param>
    /// <param name="url">Адрес страницы.</param>
    /// <returns>Разметка страницы.</returns>
    private static async Task<string> GetHtmlAsync(HttpClient client, string url)
    {
        using HttpResponseMessage response = await client.GetAsync(url);
        byte[] bytes = await response.Content.ReadAsByteArrayAsync();

        return WebUtility.HtmlDecode(Encoding.UTF8.GetString(bytes));
    }

    private static string[] ProductIds(string html) => Regex
        .Matches(html, "data-product-id=\"(?<id>[^\"]+)\"")
        .Select(match => match.Groups["id"].Value)
        .ToArray();
}