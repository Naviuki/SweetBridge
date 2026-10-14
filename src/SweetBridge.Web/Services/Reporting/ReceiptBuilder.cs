using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SweetBridge.Web.Services.Orders;

namespace SweetBridge.Web.Services.Reporting;

/// <summary>
/// Построитель накладной на QuestPDF.
/// </summary>
public sealed class ReceiptBuilder : IReceiptBuilder
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("ru-RU");

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="ReceiptBuilder"/>.
    /// </summary>
    public ReceiptBuilder()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        RegisterSystemFont();
    }

    /// <summary>
    /// Регистрирует системный шрифт: без него накладная не отрисует кириллицу.
    /// </summary>
    private static void RegisterSystemFont()
    {
        string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        string[] candidates = ["arial.ttf", "segoeui.ttf", "calibri.ttf", "tahoma.ttf"];

        foreach (string name in candidates)
        {
            string path = Path.Combine(fontsFolder, name);

            if (!File.Exists(path))
            {
                continue;
            }

            try
            {
                using FileStream stream = File.OpenRead(path);
                QuestPDF.Drawing.FontManager.RegisterFontFromStream(stream);

                return;
            }
            catch (Exception)
            {
                // Шрифт не зарегистрирован: пробуем следующий кандидат.
            }
        }
    }

    /// <inheritdoc />
    public byte[] Build(OrderSummary order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return QuestPDF.Fluent.Document.Create(content =>
        {
            content.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(style => style.FontFamily("Arial").FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text("Сладкий мост").FontSize(18).SemiBold();
                    column.Item().Text("Накладная по заказу " + order.Number);
                });

                page.Content().Column(column =>
                {
                    column.Spacing(8);

                    column.Item().Text($"Заказ: {order.Number}");
                    column.Item().Text($"Дата оформления: {order.CreatedAtUtc:dd.MM.yyyy}");
                    column.Item().Text($"Волна №{order.WaveNumber}, отправка {order.WaveShipDate:dd.MM.yyyy}");
                    column.Item().Text($"Получатель: {order.RecipientName}, {order.RecipientCity}");
                    column.Item().Text($"Пункт выдачи: {order.RecipientPickupPoint ?? "не указан"}");
                    column.Item().Text($"Статус: {StatusTitle(order.Status)} · оплата: {PaymentTitle(order.PaymentStatus)}");

                    column.Item().PaddingTop(8).LineHorizontal(1);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(240);
                            columns.ConstantColumn(50);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Товар");
                            header.Cell().Element(CellStyle).AlignRight().Text("Кол-во");
                            header.Cell().Element(CellStyle).AlignRight().Text("Цена");
                            header.Cell().Element(CellStyle).AlignRight().Text("Сумма");
                        });

                        foreach (OrderLineSummary line in order.Items)
                        {
                            table.Cell().Element(CellStyle).Text(line.ProductName);
                            table.Cell().Element(CellStyle).AlignRight().Text(line.Quantity.ToString(Culture));
                            table.Cell().Element(CellStyle).AlignRight().Text(line.UnitPrice.ToString("N0", Culture));
                            table.Cell().Element(CellStyle).AlignRight().Text(line.LineTotal.ToString("N0", Culture));
                        }

                        table.Cell().Element(CellStyle).Text("Товары");
                        table.Cell().Element(CellStyle);
                        table.Cell().Element(CellStyle);
                        table.Cell().Element(CellStyle).AlignRight().Text(order.GoodsCost.ToString("N0", Culture));

                        if (order.DiscountAmount > 0m)
                        {
                            table.Cell().Element(CellStyle).Text(
                                order.PromoCode is null ? "Скидка" : $"Скидка по промокоду {order.PromoCode}");
                            table.Cell().Element(CellStyle);
                            table.Cell().Element(CellStyle);
                            table.Cell().Element(CellStyle).AlignRight().Text("−" + order.DiscountAmount.ToString("N0", Culture));
                        }

                        table.Cell().Element(CellStyle).Text("Доставка");
                        table.Cell().Element(CellStyle);
                        table.Cell().Element(CellStyle);
                        table.Cell().Element(CellStyle).AlignRight().Text(order.ShippingCost.ToString("N0", Culture));

                        table.Cell().Element(CellStyle).Text("Термоупаковка");
                        table.Cell().Element(CellStyle);
                        table.Cell().Element(CellStyle);
                        table.Cell().Element(CellStyle).AlignRight().Text(order.ThermoPackCost.ToString("N0", Culture));

                        table.Cell().Element(CellStyle).Text("Итого").SemiBold();
                        table.Cell().Element(CellStyle);
                        table.Cell().Element(CellStyle);
                        table.Cell().Element(CellStyle).AlignRight().Text(order.Total.ToString("N0", Culture)).SemiBold();
                    });

                    column.Item().PaddingTop(10).Text(
                        "Накладная сформирована автоматически и не является кассовым чеком. "
                        + "Карточные данные платформа не принимает и не хранит.");
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Сладкий мост · hello@sweetbridge.ru · ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4);

    private static string StatusTitle(Domain.Enums.OrderStatus status) => status switch
    {
        Domain.Enums.OrderStatus.Accepted => "Принят",
        Domain.Enums.OrderStatus.Assembling => "Собирается",
        Domain.Enums.OrderStatus.InTransit => "В пути",
        Domain.Enums.OrderStatus.ReadyForPickup => "Готов к выдаче",
        Domain.Enums.OrderStatus.Received => "Получен",
        Domain.Enums.OrderStatus.Cancelled => "Отменён",
        _ => "Неизвестный статус"
    };

    private static string PaymentTitle(Domain.Enums.PaymentStatus status) => status switch
    {
        Domain.Enums.PaymentStatus.Pending => "ожидает оплаты",
        Domain.Enums.PaymentStatus.Paid => "оплачен",
        Domain.Enums.PaymentStatus.Failed => "ошибка оплаты",
        Domain.Enums.PaymentStatus.Refunded => "возвращена",
        _ => "неизвестно"
    };
}