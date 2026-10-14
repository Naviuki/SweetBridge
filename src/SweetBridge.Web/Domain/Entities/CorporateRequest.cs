using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Корпоративная заявка на крупную поставку.
/// </summary>
public sealed class CorporateRequest
{
    private CorporateRequest()
    {
    }

    private CorporateRequest(
        string company,
        string contactName,
        string email,
        string volume,
        Enums.ShippingDirection direction,
        string? comment,
        DateTime createdAtUtc)
    {
        Company = company;
        ContactName = contactName;
        Email = email;
        Volume = volume;
        Direction = direction;
        Comment = comment;
        CreatedAtUtc = createdAtUtc;
        Status = Enums.CorporateRequestStatus.New;
    }

    /// <summary>
    /// Идентификатор заявки.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Название компании.
    /// </summary>
    public string Company { get; private set; } = string.Empty;

    /// <summary>
    /// Контактное лицо.
    /// </summary>
    public string ContactName { get; private set; } = string.Empty;

    /// <summary>
    /// Почта для ответа.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Ориентировочный объём заказа.
    /// </summary>
    public string Volume { get; private set; } = string.Empty;

    /// <summary>
    /// Интересующее направление.
    /// </summary>
    public Enums.ShippingDirection Direction { get; private set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; private set; }

    /// <summary>
    /// Статус заявки.
    /// </summary>
    public Enums.CorporateRequestStatus Status { get; private set; }

    /// <summary>
    /// Момент создания, в UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Создаёт заявку.
    /// </summary>
    /// <param name="company">Компания.</param>
    /// <param name="contactName">Контактное лицо.</param>
    /// <param name="email">Почта.</param>
    /// <param name="volume">Объём.</param>
    /// <param name="direction">Направление.</param>
    /// <param name="comment">Комментарий.</param>
    /// <param name="createdAtUtc">Момент создания.</param>
    /// <returns>Новая заявка.</returns>
    /// <exception cref="DomainException">Обязательные поля не заполнены.</exception>
    public static CorporateRequest Create(
        string company,
        string contactName,
        string email,
        string volume,
        Enums.ShippingDirection direction,
        string? comment,
        DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(company) || string.IsNullOrWhiteSpace(contactName)
            || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(volume))
        {
            throw new DomainException("Заполните компанию, контактное лицо, почту и объём.");
        }

        return new CorporateRequest(
            company.Trim(),
            contactName.Trim(),
            email.Trim(),
            volume.Trim(),
            direction,
            comment?.Trim(),
            createdAtUtc);
    }

    /// <summary>
    /// Меняет статус заявки.
    /// </summary>
    /// <param name="status">Новый статус.</param>
    public void ChangeStatus(Enums.CorporateRequestStatus status) => Status = status;
}