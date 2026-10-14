using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Catalog;

/// <summary>
/// Доступ на чтение каталога.
/// </summary>
public interface ICatalogService
{
    /// <summary>
    /// Возвращает активные товары витрины, подходящие под фильтр.
    /// </summary>
    /// <param name="query">Витрина, категория и поисковый запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Подходящие товары.</returns>
    Task<IReadOnlyList<ProductSummary>> GetProductsAsync(CatalogQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает категории, используемые товарами витрины.
    /// </summary>
    /// <param name="Direction">Витрина, из которой отправляются товары.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Названия категорий в алфавитном порядке.</returns>
    Task<IReadOnlyList<string>> GetCategoriesAsync(ShippingDirection direction, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает один товар витрины.
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Товар или <see langword="null"/>, если он не найден или скрыт.</returns>
    Task<ProductSummary?> GetProductAsync(string id, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает товары одной витрины для главной страницы.
    /// </summary>
    /// <param name="direction">Витрина.</param>
    /// <param name="count">Сколько товаров вернуть.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Товары витрины.</returns>
    Task<IReadOnlyList<ProductSummary>> GetShowcaseAsync(
        ShippingDirection direction,
        int count,
        CancellationToken cancellationToken);
}
