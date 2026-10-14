namespace SweetBridge.Web.Services.Catalog;

/// <summary>
/// Модель постраничной навигации каталога.
/// </summary>
public sealed class PagerViewModel
{
    /// <summary>
    /// Номер текущей страницы, начиная с 1.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Общее количество страниц.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Общее количество товаров по текущему фильтру.
    /// </summary>
    public int TotalItems { get; init; }

    /// <summary>
    /// Есть ли предыдущая страница.
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    /// Есть ли следующая страница.
    /// </summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>
    /// Формирует ссылку на страницу с сохранением текущих фильтров.
    /// </summary>
    public required Func<int, string?> UrlFactory { get; init; }

    /// <summary>
    /// Возвращает ссылку на указанную страницу.
    /// </summary>
    /// <param name="page">Номер страницы.</param>
    /// <returns>Ссылка на страницу каталога.</returns>
    public string? BuildUrl(int page) => UrlFactory(page);
}