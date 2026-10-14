namespace SweetBridge.Web.Services.Orders;

/// <summary>
/// Модель трекера статусов заказа для страницы отслеживания и деталей.
/// </summary>
/// <param name="Steps">Подписи шагов по порядку.</param>
/// <param name="CurrentStepIndex">Индекс текущего шага, начиная с 0.</param>
public sealed record OrderTrackerViewModel(IReadOnlyList<string> Steps, int CurrentStepIndex);