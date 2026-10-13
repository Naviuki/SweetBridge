using SweetWave.Web.Domain.Enums;

namespace SweetWave.Web.Services.Catalog;

/// <summary>
/// Фильтр каталога: витрина, категория и текстовый поиск.
/// </summary>
/// <param name="Direction">Витрина, из которой отправляются товары.</param>
/// <param name="Category">Категория для фильтра или <see langword="null"/> для всех категорий.</param>
/// <param name="SearchText">Поисковый запрос по названию и производителю.</param>
public sealed record CatalogQuery(ShippingDirection Direction, string? Category = null, string? SearchText = null);
