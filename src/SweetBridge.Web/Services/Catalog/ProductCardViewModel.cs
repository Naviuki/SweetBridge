using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Catalog;

/// <summary>
/// Модель чтения карточки товара для каталога, главной страницы и подарков.
/// </summary>
/// <param name="Id">Идентификатор товара.</param>
/// <param name="Name">Наименование товара.</param>
/// <param name="Category">Категория каталога.</param>
/// <param name="ProducerName">Название производителя.</param>
/// <param name="Region">Витрина, на которой произведён товар.</param>
/// <param name="WeightGrams">Вес одной штуки в граммах.</param>
/// <param name="Price">Цена одной штуки в рублях.</param>
/// <param name="IsTemperatureSensitive">Признак термочувствительности.</param>
/// <param name="PhotoPath">Путь к фотографии или <see langword="null"/>.</param>
/// <param name="QuantityInCart">Количество товара в корзине.</param>
/// <param name="IsFavorite">Признак того, что товар в избранном.</param>
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
    int QuantityInCart,
    bool IsFavorite = false);