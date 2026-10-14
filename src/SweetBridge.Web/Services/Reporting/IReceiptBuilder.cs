namespace SweetBridge.Web.Services.Reporting;

/// <summary>
/// Построитель PDF-накладной по заказу.
/// </summary>
public interface IReceiptBuilder
{
    /// <summary>
    /// Формирует PDF с составом заказа, суммами и реквизитами волны.
    /// </summary>
    /// <param name="order">Заказ.</param>
    /// <returns>Содержимое PDF-файла.</returns>
    byte[] Build(Orders.OrderSummary order);
}