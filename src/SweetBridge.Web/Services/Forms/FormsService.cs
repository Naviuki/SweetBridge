using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Services.Common;

namespace SweetBridge.Web.Services.Forms;

/// <summary>
/// Формы обратной связи поверх базы данных.
/// </summary>
public sealed class FormsService : IFormsService
{
    private readonly AppDbContext _dbContext;
    private readonly IBusinessClock _clock;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="FormsService"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="clock">Источник времени.</param>
    public FormsService(AppDbContext dbContext, IBusinessClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(clock);

        _dbContext = dbContext;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task CreateSupportRequestAsync(
        string name,
        string email,
        string? orderNumber,
        string message,
        CancellationToken cancellationToken)
    {
        _dbContext.SupportRequests.Add(
            SupportRequest.Create(name, email, orderNumber, message, _clock.UtcNow));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateCorporateRequestAsync(
        string company,
        string contactName,
        string email,
        string volume,
        ShippingDirection direction,
        string? comment,
        CancellationToken cancellationToken)
    {
        _dbContext.CorporateRequests.Add(
            CorporateRequest.Create(company, contactName, email, volume, direction, comment, _clock.UtcNow));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string?> SubscribeAsync(string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Укажите адрес электронной почты.");
        }

        string normalized = email.Trim().ToLowerInvariant();

        NewsletterSubscriber? existing = await _dbContext.NewsletterSubscribers
            .AsNoTracking()
            .FirstOrDefaultAsync(subscriber => subscriber.Email == normalized, cancellationToken);

        if (existing is not null)
        {
            return null;
        }

        NewsletterSubscriber subscriber = NewsletterSubscriber.Subscribe(normalized, _clock.UtcNow);
        _dbContext.NewsletterSubscribers.Add(subscriber);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return subscriber.ConfirmationToken;
    }

    /// <inheritdoc />
    public async Task<bool> ConfirmSubscriptionAsync(string? token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        NewsletterSubscriber[] subscribers = await _dbContext.NewsletterSubscribers
            .Where(subscriber => subscriber.ConfirmationToken == token)
            .ToArrayAsync(cancellationToken);

        if (subscribers.Length == 0)
        {
            return false;
        }

        foreach (NewsletterSubscriber subscriber in subscribers)
        {
            subscriber.Confirm(token);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<FormsOverview> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var supportRequests = await _dbContext.SupportRequests
            .AsNoTracking()
            .OrderByDescending(request => request.CreatedAtUtc)
            .Take(200)
            .Select(request => new SupportRequestSummary(
                request.Id,
                request.Name,
                request.Email,
                request.OrderNumber,
                request.Message,
                request.Status,
                request.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        var corporateRequests = await _dbContext.CorporateRequests
            .AsNoTracking()
            .OrderByDescending(request => request.CreatedAtUtc)
            .Take(200)
            .Select(request => new CorporateRequestSummary(
                request.Id,
                request.Company,
                request.ContactName,
                request.Email,
                request.Volume,
                request.Direction,
                request.Status,
                request.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        int subscribers = await _dbContext.NewsletterSubscribers.CountAsync(cancellationToken);

        return new FormsOverview(supportRequests, corporateRequests, subscribers);
    }

    /// <inheritdoc />
    public async Task ChangeStatusAsync(FormEntityKind kind, int id, string status, CancellationToken cancellationToken)
    {
        if (kind == FormEntityKind.SupportRequest)
        {
            SupportRequest? request = await _dbContext.SupportRequests
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
                ?? throw new DomainException("Обращение не найдено.");

            request.ChangeStatus(ParseSupportStatus(status));

            await _dbContext.SaveChangesAsync(cancellationToken);

            return;
        }

        CorporateRequest? corporate = await _dbContext.CorporateRequests
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new DomainException("Корпоративная заявка не найдена.");

        corporate.ChangeStatus(ParseCorporateStatus(status));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SupportRequestStatus ParseSupportStatus(string status) =>
        Enum.TryParse(status, ignoreCase: true, out SupportRequestStatus parsed)
            ? parsed
            : SupportRequestStatus.New;

    private static CorporateRequestStatus ParseCorporateStatus(string status) =>
        Enum.TryParse(status, ignoreCase: true, out CorporateRequestStatus parsed)
            ? parsed
            : CorporateRequestStatus.New;
}