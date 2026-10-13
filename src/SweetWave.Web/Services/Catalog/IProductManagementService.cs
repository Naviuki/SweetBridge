namespace SweetWave.Web.Services.Catalog;

/// <summary>
/// Доступ админки к каталогу: создание, правка и видимость товаров.
/// </summary>
public interface IProductManagementService
{
    /// <summary>
    /// Возвращает все товары, включая скрытые.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Все товары каталога.</returns>
    Task<IReadOnlyList<ProductSummary>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Создаёт новый товар.
    /// </summary>
    /// <param name="input">Данные товара.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданный товар.</returns>
    Task<ProductSummary> CreateAsync(ProductInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Обновляет существующий товар.
    /// </summary>
    /// <param name="Id">Идентификатор товара.</param>
    /// <param name="input">Новые данные товара.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Обновлённый товар.</returns>
    Task<ProductSummary> UpdateAsync(string id, ProductInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Скрывает товар или возвращает его в каталог.
    /// </summary>
    /// <param name="Id">Идентификатор товара.</param>
    /// <param name="isVisible">Требуемая видимость.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task SetVisibilityAsync(string id, bool isVisible, CancellationToken cancellationToken);
}
