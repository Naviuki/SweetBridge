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
}
