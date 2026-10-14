using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SweetBridge.Web.Infrastructure.Persistence;

namespace SweetBridge.Web.Infrastructure.Health;

/// <summary>
/// Проверка готовности: база данных доступна и миграции применены.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="DatabaseHealthCheck"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public DatabaseHealthCheck(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("База данных доступна.")
                : HealthCheckResult.Unhealthy("Нет соединения с базой данных.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Ошибка проверки базы данных.", exception);
        }
    }
}