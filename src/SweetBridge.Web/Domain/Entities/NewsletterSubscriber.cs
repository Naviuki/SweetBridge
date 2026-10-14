using System.Security.Cryptography;
using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.Entities;

/// <summary>
/// Подписчик рассылки о новых волнах.
/// </summary>
public sealed class NewsletterSubscriber
{
    private NewsletterSubscriber()
    {
    }

    private NewsletterSubscriber(string email, DateTime createdAtUtc)
    {
        Email = email;
        CreatedAtUtc = createdAtUtc;
        ConfirmationToken = TokenGenerator.Create();
        UnsubscribeToken = TokenGenerator.Create();
    }

    /// <summary>
    /// Идентификатор подписчика.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Почта подписчика в нижнем регистре.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Признак подтверждённой подписки.
    /// </summary>
    public bool IsConfirmed { get; private set; }

    /// <summary>
    /// Токен подтверждения подписки.
    /// </summary>
    public string ConfirmationToken { get; private set; } = string.Empty;

    /// <summary>
    /// Токен отписки.
    /// </summary>
    public string UnsubscribeToken { get; private set; } = string.Empty;

    /// <summary>
    /// Момент подписки, в UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Подписывает адрес.
    /// </summary>
    /// <param name="email">Почта.</param>
    /// <param name="createdAtUtc">Момент подписки.</param>
    /// <returns>Новый подписчик.</returns>
    /// <exception cref="DomainException">Почта не заполнена.</exception>
    public static NewsletterSubscriber Subscribe(string email, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Укажите адрес электронной почты.");
        }

        return new NewsletterSubscriber(email.Trim().ToLowerInvariant(), createdAtUtc);
    }

    /// <summary>
    /// Подтверждает подписку по токену.
    /// </summary>
    /// <param name="token">Токен из письма.</param>
    /// <returns><see langword="true"/>, если подписка подтверждена.</returns>
    public bool Confirm(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)
            || !CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(token),
                System.Text.Encoding.UTF8.GetBytes(ConfirmationToken)))
        {
            return false;
        }

        IsConfirmed = true;

        return true;
    }

    /// <summary>
    /// Проверяет токен отписки.
    /// </summary>
    /// <param name="token">Токен из ссылки отписки.</param>
    /// <returns><see langword="true"/>, если токен верный.</returns>
    public bool IsUnsubscribeToken(string? token) =>
        !string.IsNullOrWhiteSpace(token)
        && CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(token),
            System.Text.Encoding.UTF8.GetBytes(UnsubscribeToken));

    private static class TokenGenerator
    {
        public static string Create() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
    }
}