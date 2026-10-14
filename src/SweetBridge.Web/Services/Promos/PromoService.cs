using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Services.Common;

namespace SweetBridge.Web.Services.Promos;

/// <summary>
/// Проверка и применение промокодов поверх базы данных.
/// </summary>
public sealed class PromoService : IPromoService
{
    private readonly AppDbContext _dbContext;
    private readonly IBusinessClock _clock;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="PromoService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="clock">Источник текущей даты.</param>
    public PromoService(AppDbContext dbContext, IBusinessClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(clock);

        _dbContext = dbContext;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<PromoApplyResult> ApplyAsync(string? code, decimal goodsTotal, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return PromoApplyResult.None();
        }

        string normalized = code.Trim().ToUpperInvariant();
        PromoCode? promo = await _dbContext.PromoCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Code == normalized, cancellationToken);

        if (promo is null)
        {
            return new PromoApplyResult(false, "Промокод не найден. Проверьте написание.", 0m);
        }

        if (!promo.IsActive)
        {
            return new PromoApplyResult(false, "Промокод больше не действует.", 0m);
        }

        DateOnly today = _clock.LocalToday;
        if (today < promo.ValidFrom)
        {
            return new PromoApplyResult(false, $"Промокод начнёт действовать {promo.ValidFrom:dd.MM.yyyy}.", 0m);
        }

        if (today > promo.ValidTo)
        {
            return new PromoApplyResult(false, "Срок действия промокода истёк.", 0m);
        }

        if (promo.MaxUses > 0 && promo.UsedCount >= promo.MaxUses)
        {
            return new PromoApplyResult(false, "Промокод исчерпал лимит применений.", 0m);
        }

        if (goodsTotal < promo.MinOrderAmount)
        {
            return new PromoApplyResult(false, $"Промокод действует от {promo.MinOrderAmount:N0} ₽.", 0m);
        }

        decimal discount = promo.CalculateDiscount(goodsTotal);

        return new PromoApplyResult(true, $"Промокод применён: скидка {discount:N0} ₽.", discount);
    }

    /// <inheritdoc />
    public async Task RegisterUsageAsync(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        string normalized = code.Trim().ToUpperInvariant();
        PromoCode? promo = await _dbContext.PromoCodes
            .FirstOrDefaultAsync(item => item.Code == normalized, cancellationToken);

        if (promo is null)
        {
            return;
        }

        promo.RegisterUsage();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}