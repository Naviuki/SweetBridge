using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SweetBridge.Tests.Integration;

/// <summary>
/// Тестовый сервер с отдельной временной базой: тесты не трогают базу разработки.
/// </summary>
public sealed class SiteWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        "sweetbridge-tests",
        $"tests-{Guid.NewGuid():N}.db");

    /// <summary>
    /// Настраивает окружение тестового сервера.
    /// </summary>
    /// <param name="builder">Конфигурация веб-хоста.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);

        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_databasePath}");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_databasePath}",
                ["Seed:AdminEmail"] = "admin@tests.sweetbridge.local"
            }));
    }
}
