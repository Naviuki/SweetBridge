using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Services.Catalog;

namespace SweetWave.Web.Services.Waves;

/// <summary>
/// Волны: открытые предложения для страницы оформления и управление расписанием волн.
/// </summary>
public interface IWaveService
{
    /// <summary>
    /// Рассчитывает предложения всех волн указанного направления, которые ещё принимают заказы.
    /// </summary>
    /// <param name="direction">Направление доставки корзины.</param>
    /// <param name="products">Товары корзины.</param>
    /// <param name="orderWeightGrams">Вес корзины в граммах.</param>
    /// <param name="thermoPackRequested">Термоупаковка, выбранная покупателем.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Предложения, отсортированные по дате отправки.</returns>
    Task<IReadOnlyList<WaveOffer>> GetOpenOffersAsync(
        ShippingDirection direction,
        IReadOnlyList<ProductSummary> products,
        int orderWeightGrams,
        bool thermoPackRequested,
        CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает ближайшие волны, которые ещё принимают заказы, без расчёта доставки.
    /// </summary>
    /// <param name="direction">Направление доставки.</param>
    /// <param name="count">Максимальное количество возвращаемых волн.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ближайшие волны, отсортированные по дате отправки.</returns>
    Task<IReadOnlyList<WaveSummary>> GetUpcomingAsync(
        ShippingDirection direction,
        int count,
        CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает все волны с заполнением для списка админки.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Все волны, отсортированные по дате отправки.</returns>
    Task<IReadOnlyList<WaveSummary>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Создаёт запланированную волну: дедлайн приёма заказов и ограничения загрузки берутся из конфигурации.
    /// </summary>
    /// <param name="command">Направление и дата отправки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданная волна.</returns>
    Task<WaveSummary> CreateAsync(CreateWaveCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Отправляет волну и переводит все её заказы в статус «в пути».
    /// </summary>
    /// <param name="waveId">Идентификатор волны.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task DispatchAsync(int waveId, CancellationToken cancellationToken);
}
