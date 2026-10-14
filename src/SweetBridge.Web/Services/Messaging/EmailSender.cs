using System.Globalization;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SweetBridge.Web.Options;

namespace SweetBridge.Web.Services.Messaging;

/// <summary>
/// Отправка писем через SMTP или через файловый outbox, если SMTP не настроен.
/// </summary>
public sealed class EmailSender : IEmailSender
{
    private const string OutboxMode = "Outbox";

    private readonly EmailOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<EmailSender> _logger;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="EmailSender"/>.
    /// </summary>
    /// <param name="options">Параметры почты.</param>
    /// <param name="environment">Среда выполнения: определяет базовый путь для outbox.</param>
    /// <param name="logger">Журнал событий. Письма и адреса в журнал не попадают.</param>
    public EmailSender(IOptions<EmailOptions> options, IHostEnvironment environment, ILogger<EmailSender> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendAsync(OutgoingEmail email, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (string.IsNullOrWhiteSpace(_options.Smtp.Host))
        {
            WriteToOutbox(email);

            return;
        }

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_options.FromAddress));
        message.To.Add(MailboxAddress.Parse(email.To));
        message.Subject = email.Subject;
        message.Body = new TextPart("plain") { Text = email.Body };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _options.Smtp.Host,
                _options.Smtp.Port,
                SecureSocketOptions.StartTlsWhenAvailable,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.Smtp.UserName))
            {
                await client.AuthenticateAsync(_options.Smtp.UserName, _options.Smtp.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
        }
        catch (SmtpCommandException exception)
        {
            _logger.LogError(exception, "Не удалось отправить письмо через SMTP: {Host}", _options.Smtp.Host);
            WriteToOutbox(email);

            throw;
        }
    }

    private void WriteToOutbox(OutgoingEmail email)
    {
        string directory = Path.Combine(_environment.ContentRootPath, _options.OutboxPath);
        Directory.CreateDirectory(directory);

        string fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}.eml";
        string path = Path.Combine(directory, fileName);

        var builder = new StringBuilder();
        builder.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"Date: {DateTimeOffset.UtcNow:R}");
        builder.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"From: {_options.FromAddress}");
        builder.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"To: {email.To}");
        builder.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"Subject: {email.Subject}");
        builder.AppendLine("Content-Type: text/plain; charset=utf-8");
        builder.AppendLine();
        builder.AppendLine(email.Body);

        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        _logger.LogInformation("Письмо сохранено в outbox: {FileName}", fileName);
    }
}

/// <summary>
/// Формирование писем приложения.
/// </summary>
public sealed class EmailComposer : IEmailComposer
{
    private readonly IEmailSender _sender;
    private readonly ILogger<EmailComposer> _logger;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="EmailComposer"/>.
    /// </summary>
    /// <param name="sender">Отправитель писем.</param>
    /// <param name="logger">Журнал событий.</param>
    public EmailComposer(IEmailSender sender, ILogger<EmailComposer> logger)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(logger);

        _sender = sender;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task SendOrderConfirmationAsync(
        string recipient,
        string orderNumber,
        decimal total,
        string paymentUrl,
        CancellationToken cancellationToken)
    {
        string body = string.Create(
            CultureInfo.CurrentCulture,
            $"Здравствуйте!\n\nВаш заказ {orderNumber} принят в работу.\nСумма к оплате: {total:N0} ₽.\n\nОплатить заказ: {paymentUrl}\n\n«Сладкий мост» — доставка волнами между Якутией и Донбассом.");

        return _sender.SendAsync(new OutgoingEmail(recipient, $"Заказ {orderNumber} принят", body), cancellationToken);
    }

    /// <inheritdoc />
    public Task SendOrderStatusAsync(
        string recipient,
        string orderNumber,
        string statusTitle,
        string details,
        string trackUrl,
        CancellationToken cancellationToken)
    {
        string body = string.Create(
            CultureInfo.CurrentCulture,
            $"Статус заказа {orderNumber} изменился: {statusTitle}.\n\n{details}\n\nСледить за заказом: {trackUrl}");

        return _sender.SendAsync(new OutgoingEmail(recipient, $"Заказ {orderNumber}: {statusTitle}", body), cancellationToken);
    }

    /// <inheritdoc />
    public Task SendSubscriptionConfirmationAsync(string recipient, string confirmationUrl, CancellationToken cancellationToken)
    {
        string body = $"Подтвердите подписку на новости «Сладкого моста»: {confirmationUrl}\n\nЕсли вы не оставляли адрес, просто проигнорируйте письмо.";

        return _sender.SendAsync(new OutgoingEmail(recipient, "Подтвердите подписку на рассылку", body), cancellationToken);
    }

    /// <inheritdoc />
    public Task SendPasswordResetAsync(string recipient, string resetUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Запрошен сброс пароля для {Recipient}", MaskEmail(recipient));

        string body = $"Для смены пароля откройте ссылку: {resetUrl}\n\nЕсли вы не запрашивали смену пароля, просто проигнорируйте письмо.";

        return _sender.SendAsync(new OutgoingEmail(recipient, "Смена пароля в «Сладком мосте»", body), cancellationToken);
    }

    private static string MaskEmail(string email)
    {
        int at = email.IndexOf('@', StringComparison.Ordinal);
        if (at <= 0)
        {
            return "***";
        }

        return string.Concat(email[..1], "***", email[at..]);
    }
}