namespace SweetBridge.Web.Services.Messaging;

/// <summary>
/// Письмо, подготовленное к отправке.
/// </summary>
/// <param name="To">Получатель.</param>
/// <param name="Subject">Тема.</param>
/// <param name="Body">Текст письма.</param>
public sealed record OutgoingEmail(string To, string Subject, string Body);

/// <summary>
/// Отправка писем: SMTP или файловый outbox для режима разработки.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Отправляет письмо.
    /// </summary>
    /// <param name="email">Письмо.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task SendAsync(OutgoingEmail email, CancellationToken cancellationToken);
}

/// <summary>
/// Письма, отправляемые приложением.
/// </summary>
public interface IEmailComposer
{
    /// <summary>
    /// Письмо с подтверждением заказа и реквизитами оплаты.
    /// </summary>
    /// <param name="to">Получатель.</param>
    /// <param name="orderNumber">Номер заказа.</param>
    /// <param name="total">Сумма к оплате.</param>
    /// <param name="paymentUrl">Ссылка на оплату.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task SendOrderConfirmationAsync(string recipient, string orderNumber, decimal total, string paymentUrl, CancellationToken cancellationToken);

    /// <summary>
    /// Письмо со сменой статуса заказа.
    /// </summary>
    /// <param name="to">Получатель.</param>
    /// <param name="orderNumber">Номер заказа.</param>
    /// <param name="statusTitle">Заголовок статуса.</param>
    /// <param name="details">Подробности статуса.</param>
    /// <param name="trackUrl">Ссылка на отслеживание.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task SendOrderStatusAsync(
        string recipient,
        string orderNumber,
        string statusTitle,
        string details,
        string trackUrl,
        CancellationToken cancellationToken);

    /// <summary>
    /// Письмо для подтверждения подписки на рассылку.
    /// </summary>
    /// <param name="to">Получатель.</param>
    /// <param name="confirmationUrl">Ссылка подтверждения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task SendSubscriptionConfirmationAsync(string recipient, string confirmationUrl, CancellationToken cancellationToken);

    /// <summary>
    /// Письмо для сброса пароля.
    /// </summary>
    /// <param name="to">Получатель.</param>
    /// <param name="resetUrl">Ссылка сброса.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    Task SendPasswordResetAsync(string recipient, string resetUrl, CancellationToken cancellationToken);
}