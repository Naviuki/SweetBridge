using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Catalog;

/// <summary>
/// Модель чтения карточки товара для каталога и главной страницы.
/// </summary>
public sealed record ProductCardViewModel(
    string Id,
    string Name,
    string Category,
    string ProducerName,
    Region Region,
    int WeightGrams,
    decimal Price,
    bool IsTemperatureSensitive,
    string? PhotoPath,
    int QuantityInCart);