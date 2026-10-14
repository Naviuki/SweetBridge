using Microsoft.Extensions.Options;
using SweetBridge.Web.Options;

namespace SweetBridge.Web.Services.Common;

/// <summary>
/// Источник текущего времени в часовом поясе региона.
/// Позволяет считать даты волн и дедлайны в местном времени, а не в UTC сервера.
/// </summary>
public interface IBusinessClock
{
    /// <summary>
    /// Текущий момент в UTC.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Текущий момент в местном времени региона.
    /// </summary>
    DateTime LocalNow { get; }

    /// <summary>
    /// Текущая дата в местном времени региона.
    /// </summary>
    DateOnly LocalToday { get; }
}

/// <summary>
/// Реализация <see cref="IBusinessClock"/> поверх часового пояса из конфигурации.
/// </summary>
public sealed class BusinessClock : IBusinessClock
{
    private readonly TimeZoneInfo _timeZone;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="BusinessClock"/>.
    /// </summary>
    /// <param name="options">Логистические параметры с идентификатором часового пояса.</param>
    public BusinessClock(IOptions<LogisticsOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _timeZone = ResolveTimeZone(options.Value.TimeZoneId);
    }

    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc />
    public DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);

    /// <inheritdoc />
    public DateOnly LocalToday => DateOnly.FromDateTime(LocalNow);

    /// <summary>
    /// Преобразует момент UTC в местное время региона.
    /// </summary>
    /// <param name="utc">Момент в UTC.</param>
    /// <returns>Момент в местном времени региона.</returns>
    public DateTime ToLocal(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(utc, _timeZone);

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.Utc;
        }

        return TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out TimeZoneInfo? zone)
            ? zone
            : throw new InvalidOperationException(
                $"Часовой пояс '{timeZoneId}' не найден в системе. Проверьте параметр Logistics:TimeZoneId.");
    }
}