using SweetBridge.Web.Services.Orders;

namespace SweetBridge.Web.Infrastructure.Background;

/// <summary>
/// Фоновая задача: отменяет заказы с истёкшим резервом оплаты и освобождает вес в волнах.
/// </summary>
public sealed partial class UnpaidOrderCleanupService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UnpaidOrderCleanupService> _logger;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="UnpaidOrderCleanupService"/>.
    /// </summary>
    /// <param name="scopeFactory">Фабрика областей внедрения.</param>
    /// <param name="logger">Журнал событий.</param>
    public UnpaidOrderCleanupService(IServiceScopeFactory scopeFactory, ILogger<UnpaidOrderCleanupService> logger)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
                IOrderService orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                int released = await orderService.ReleaseExpiredOrdersAsync(stoppingToken);

                if (released > 0)
                {
                    LogReleased(_logger, released);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                LogCleanupFailed(_logger, exception);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Освобождено заказов без оплаты: {Count}")]
    private static partial void LogReleased(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Не удалось освободить заказы без оплаты")]
    private static partial void LogCleanupFailed(ILogger logger, Exception exception);
}