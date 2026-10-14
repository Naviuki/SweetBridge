using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Services.Common;

namespace SweetBridge.Web.Services.Forms;

/// <summary>
/// Обращения в поддержку, корпоративные заявки и подписка на рассылку.
/// </summary>
public interface IFormsService
{
    /// <summary>
    /// Сохраняет обращение в поддержку.
    /// </summary>
    /// <param name="name">Имя отправителя.</param>
    /// <param name="email">Почта.</param>
    /// <param name="orderNumber">Номер заказа.</param>
    /// <param name="message">Текст обращения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task CreateSupportRequestAsync(string name, string email, string? orderNumber, string message, CancellationToken cancellationToken);

    /// <summary>
    /// Сохраняет корпоративную заявку.
    /// </summary>
    /// <param name="company">Компания.</param>
    /// <param name="contactName">Контактное лицо.</param>
    /// <param name="email">Почта.</param>
    /// <param name="volume">Объём.</param>
    /// <param name="direction">Направление.</param>
    /// <param name="comment">Комментарий.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task CreateCorporateRequestAsync(
        string company,
        string contactName,
        string email,
        string volume,
        ShippingDirection direction,
        string? comment,
        CancellationToken cancellationToken);

    /// <summary>
    /// Подписывает адрес на рассылку. Повторная подписка не создаёт дубликат.
    /// </summary>
    /// <param name="email">Адрес.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Токен подтверждения для письма или <see langword="null"/>, если адрес уже был.</returns>
    Task<string?> SubscribeAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Подтверждает подписку.
    /// </summary>
    /// <param name="token">Токен из письма.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns><see langword="true"/>, если подписка подтверждена.</returns>
    Task<bool> ConfirmSubscriptionAsync(string? token, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает списки обращений и заявок для админки.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Обращения, заявки и число подписчиков.</returns>
    Task<FormsOverview> GetOverviewAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Меняет статус обращения или корпоративной заявки.
    /// </summary>
    /// <param name="kind">Тип объекта.</param>
    /// <param name="id">Идентификатор.</param>
    /// <param name="status">Новый статус.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task ChangeStatusAsync(FormEntityKind kind, int id, string status, CancellationToken cancellationToken);
}

/// <summary>
/// Тип объекта формы в админке.
/// </summary>
public enum FormEntityKind
{
    /// <summary>
    /// Обращение в поддержку.
    /// </summary>
    SupportRequest = 0,

    /// <summary>
    /// Корпоративная заявка.
    /// </summary>
    CorporateRequest = 1
}

/// <summary>
/// Модель чтения обращения в поддержку.
/// </summary>
/// <param name="Id">Идентификатор.</param>
/// <param name="Name">Имя.</param>
/// <param name="Email">Почта.</param>
/// <param name="OrderNumber">Номер заказа.</param>
/// <param name="Message">Текст.</param>
/// <param name="Status">Статус.</param>
/// <param name="CreatedAtUtc">Момент создания.</param>
public sealed record SupportRequestSummary(
    int Id,
    string Name,
    string Email,
    string? OrderNumber,
    string Message,
    SupportRequestStatus Status,
    DateTime CreatedAtUtc);

/// <summary>
/// Модель чтения корпоративной заявки.
/// </summary>
/// <param name="Id">Идентификатор.</param>
/// <param name="Company">Компания.</param>
/// <param name="ContactName">Контактное лицо.</param>
/// <param name="Email">Почта.</param>
/// <param name="Volume">Объём.</param>
/// <param name="Direction">Направление.</param>
/// <param name="Status">Статус.</param>
/// <param name="CreatedAtUtc">Момент создания.</param>
public sealed record CorporateRequestSummary(
    int Id,
    string Company,
    string ContactName,
    string Email,
    string Volume,
    ShippingDirection Direction,
    CorporateRequestStatus Status,
    DateTime CreatedAtUtc);

/// <summary>
/// Сводка форм для админки.
/// </summary>
/// <param name="SupportRequests">Обращения в поддержку.</param>
/// <param name="CorporateRequests">Корпоративные заявки.</param>
/// <param name="SubscriberCount">Число подписчиков.</param>
public sealed record FormsOverview(
    IReadOnlyList<SupportRequestSummary> SupportRequests,
    IReadOnlyList<CorporateRequestSummary> CorporateRequests,
    int SubscriberCount);