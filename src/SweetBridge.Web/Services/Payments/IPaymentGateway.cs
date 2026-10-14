namespace SweetBridge.Web.Services.Payments;

/// <summary>
/// Результат попытки оплаты.
/// </summary>
/// <param name="IsSuccess">Признак успешной оплаты.</param>
/// <param name="Message">Сообщение для покупателя.</param>
/// <param name="RedirectUrl">Адрес, на который нужно перейти после оплаты.</param>
public sealed record PaymentResult(bool IsSuccess, string Message, string? RedirectUrl);

/// <summary>
/// Демонстрационный платёжный шлюз.
/// Реальные карточные данные не принимаются и не хранятся: шлюз имитирует проведение оплаты.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Создаёт платёж и возвращает адрес страницы оплаты.
    /// </summary>
    /// <param name="orderNumber">Номер заказа.</param>
    /// <param name="amount">Сумма к оплате.</param>
    /// <returns>Адрес страницы оплаты.</returns>
    string CreatePaymentPageUrl(string orderNumber, decimal amount);

    /// <summary>
    /// Проводит оплату заказа.
    /// </summary>
    /// <param name="orderNumber">Номер заказа.</param>
    /// <param name="amount">Сумма к оплате.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат оплаты.</returns>
    Task<PaymentResult> PayAsync(string orderNumber, decimal amount, CancellationToken cancellationToken);
}

/// <summary>
/// Демонстрационный шлюз: оплата всегда проходит успешно.
/// </summary>
public sealed class DemoPaymentGateway : IPaymentGateway
{
    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="DemoPaymentGateway"/>.
    /// </summary>
    /// <param name="linkGenerator">Генератор ссылок на страницы приложения.</param>
    public DemoPaymentGateway(LinkGenerator linkGenerator)
    {
        ArgumentNullException.ThrowIfNull(linkGenerator);

        LinkGenerator = linkGenerator;
    }

    private LinkGenerator LinkGenerator { get; }

    /// <inheritdoc />
    public string CreatePaymentPageUrl(string orderNumber, decimal amount) =>
        LinkGenerator.GetPathByPage("/Checkout/Pay", values: new { number = orderNumber, amount }) ?? "/Checkout";

    /// <inheritdoc />
    public Task<PaymentResult> PayAsync(string orderNumber, decimal amount, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);

        if (amount <= 0m)
        {
            return Task.FromResult(new PaymentResult(false, "Сумма к оплате должна быть больше нуля.", null));
        }

        return Task.FromResult(new PaymentResult(
            true,
            $"Оплата заказа {orderNumber} на сумму {amount:N0} ₽ прошла успешно.",
            null));
    }
}