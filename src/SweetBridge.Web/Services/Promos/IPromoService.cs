using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Infrastructure.Persistence;

namespace SweetBridge.Web.Services.Promos;

/// <summary>
/// Проверка и применение промокодов.
/// </summary>
public interface IPromoService
{
    /// <summary>
    /// Проверяет промокод и возвращает результат применения к сумме товаров.
    /// </summary>
    /// <param name="code">Код промокода.</param>
    /// <param name="goodsTotal">Сумма товаров в рублях.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат применения промокода.</returns>
    Task<PromoApplyResult> ApplyAsync(string? code, decimal goodsTotal, CancellationToken cancellationToken);

    /// <summary>
    /// Фиксирует использование промокода после успешного оформления заказа.
    /// </summary>
    /// <param name="code">Код промокода.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task RegisterUsageAsync(string code, CancellationToken cancellationToken);
}

/// <summary>
/// Результат проверки промокода.
/// </summary>
/// <param name="IsSuccess">Признак успешной проверки.</param>
/// <param name="Message">Сообщение для пользователя.</param>
/// <param name="DiscountAmount">Размер скидки в рублях.</param>
public sealed record PromoApplyResult(bool IsSuccess, string Message, decimal DiscountAmount)
{
    /// <summary>
    /// Пустой успешный результат.
    /// </summary>
    public static PromoApplyResult None() => new(true, string.Empty, 0m);
}