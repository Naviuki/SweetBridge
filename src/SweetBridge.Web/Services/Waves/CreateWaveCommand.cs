using SweetBridge.Web.Domain.Enums;

namespace SweetBridge.Web.Services.Waves;

/// <summary>
/// Данные волны, создаваемой в админке. Вместимость и минимальная загрузка берутся из конфигурации.
/// </summary>
/// <param name="direction">Направление доставки.</param>
/// <param name="shipDate">Запланированная дата отправки.</param>
public sealed record CreateWaveCommand(ShippingDirection Direction, DateOnly ShipDate);
