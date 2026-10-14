using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Infrastructure.Persistence;
using SweetBridge.Web.Options;
using SweetBridge.Web.Services.Cart;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Common;
using SweetBridge.Web.Services.Messaging;
using SweetBridge.Web.Services.Orders;
using SweetBridge.Web.Services.Promos;
using SweetBridge.Web.Services.Waves;

namespace SweetBridge.Web.Pages.Checkout;

/// <summary>
/// Оформление заказа: данные покупателя и получателя, пункт выдачи, промокод и выбор волны.
/// </summary>
/// <param name="cartService">Сервис корзины.</param>
/// <param name="waveService">Сервис волн.</param>
/// <param name="orderService">Сервис заказов.</param>
/// <param name="promoService">Сервис промокодов.</param>
/// <param name="emailComposer">Формировщик писем.</param>
/// <param name="dbContext">Контекст базы данных для списка пунктов выдачи.</param>
public sealed class IndexModel(
    ICartService cartService,
    IWaveService waveService,
    IOrderService orderService,
    IPromoService promoService,
    IEmailComposer emailComposer,
    AppDbContext dbContext) : PageModel
{
    /// <summary>
    /// Данные формы оформления заказа.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Текущая корзина.
    /// </summary>
    public CartView Cart { get; private set; } = CartView.Empty();

    /// <summary>
    /// Волны, которые ещё принимают заказы для корзины.
    /// </summary>
    public IReadOnlyList<WaveOffer> Offers { get; private set; } = [];

    /// <summary>
    /// Пункты выдачи в регионе назначения.
    /// </summary>
    public IReadOnlyList<PickupPointSummary> PickupPoints { get; private set; } = [];

    /// <summary>
    /// Загружает корзину и рассчитывает предложения открытых волн.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Cart = await cartService.GetCartAsync(cancellationToken);

        if (Cart.IsEmpty)
        {
            return RedirectToPage("/Cart/Index");
        }

        await LoadAsync(cancellationToken);

        return Page();
    }

    /// <summary>
    /// Размещает заказ и перенаправляет на страницу подтверждения.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на страницу подтверждения.</returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Cart = await cartService.GetCartAsync(cancellationToken);

        if (Cart.IsEmpty)
        {
            return RedirectToPage("/Cart/Index");
        }

        await LoadAsync(cancellationToken);

        if (Input.WaveId <= 0)
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.WaveId)}", "Выберите волну доставки.");
        }

        if (Input.PickupPointId <= 0)
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.PickupPointId)}", "Выберите пункт выдачи.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        string? userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        PickupPointSummary? pickupPoint = PickupPoints.FirstOrDefault(point => point.Id == Input.PickupPointId);
        if (pickupPoint is null)
        {
            ModelState.AddModelError(string.Empty, "Выбранный пункт выдачи недоступен.");

            return Page();
        }

        PlaceOrderCommand command = new(
            Input.WaveId,
            userId,
            Input.BuyerName,
            Input.BuyerPhone,
            Input.IsGift,
            Input.IsGift ? Input.RecipientName : null,
            Input.IsGift ? Input.RecipientPhone : null,
            Input.RecipientCity,
            Input.PickupPointId,
            pickupPoint.DisplayName,
            Input.GiftMessage,
            Input.ThermoPackRequested,
            string.IsNullOrWhiteSpace(Input.PromoCode) ? null : Input.PromoCode,
            [.. Cart.Lines.Select(line => new OrderLineRequest(line.Product.Id, line.Quantity))]);

        try
        {
            OrderSummary order = await orderService.PlaceAsync(command, cancellationToken);

            cartService.Clear();
            TempData["Success"] = "Заказ оформлен. Вес зарезервирован в волне до оплаты.";

            await emailComposer.SendOrderConfirmationAsync(
                Input.Email,
                order.Number,
                order.Total,
                Url.Page("/Checkout/Pay", new { number = order.Number }) ?? "/Checkout/Index",
                cancellationToken);

            return RedirectToPage("/Orders/Confirmation", new { number = order.Number });
        }
        catch (DomainException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadAsync(cancellationToken);

            return Page();
        }
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        ShippingDirection direction = Cart.ShippingDirection ?? ShippingDirection.YakutiaToDonbass;

        Offers = await waveService.GetOpenOffersAsync(
            direction,
            Cart.Lines.Select(line => line.Product).ToList(),
            Cart.TotalWeightGrams,
            Input.ThermoPackRequested,
            cancellationToken);

        Region destinationRegion = direction == ShippingDirection.YakutiaToDonbass ? Region.Donbass : Region.Yakutia;

        PickupPoints = await dbContext.PickupPoints
            .AsNoTracking()
            .Where(point => point.IsActive && point.Region == destinationRegion)
            .OrderBy(point => point.City)
            .ThenBy(point => point.Number)
            .Select(point => new PickupPointSummary(
                point.Id,
                point.Region,
                point.City,
                point.Number,
                point.Address))
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(Input.PromoCode))
        {
            PromoApplyResult promo = await promoService.ApplyAsync(Input.PromoCode, Cart.GoodsCost, cancellationToken);
            PromoMessage = promo.Message;
            PromoDiscount = promo.IsSuccess ? promo.DiscountAmount : 0m;
        }
    }

    /// <summary>
    /// Сообщение по промокоду.
    /// </summary>
    public string? PromoMessage { get; private set; }

    /// <summary>
    /// Скидка по промокоду в рублях.
    /// </summary>
    public decimal PromoDiscount { get; private set; }

    /// <summary>
    /// Пункт выдачи для выбора при оформлении.
    /// </summary>
    /// <param name="Id">Идентификатор.</param>
    /// <param name="Region">Регион.</param>
    /// <param name="City">Город.</param>
    /// <param name="Number">Номер пункта.</param>
    /// <param name="Address">Адрес.</param>
    public sealed record PickupPointSummary(int Id, Region Region, string City, string Number, string Address)
    {
        /// <summary>
        /// Название пункта выдачи для показа.
        /// </summary>
        public string DisplayName => $"{City}, {Number} · {Address}";
    }

    /// <summary>
    /// Форма оформления с русскими сообщениями валидации.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Имя покупателя.
        /// </summary>
        [Required(ErrorMessage = "Укажите имя покупателя.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Имя покупателя должно быть от 2 до 200 символов.")]
        [Display(Name = "Имя покупателя")]
        public string BuyerName { get; set; } = string.Empty;

        /// <summary>
        /// Телефон покупателя.
        /// </summary>
        [Required(ErrorMessage = "Укажите телефон покупателя.")]
        [StringLength(40, ErrorMessage = "Телефон не должен быть длиннее 40 символов.")]
        [Display(Name = "Телефон покупателя")]
        public string BuyerPhone { get; set; } = string.Empty;

        /// <summary>
        /// Почта покупателя для уведомлений.
        /// </summary>
        [Required(ErrorMessage = "Укажите почту для уведомлений.")]
        [EmailAddress(ErrorMessage = "Проверьте адрес электронной почты.")]
        [StringLength(320, ErrorMessage = "Почта не должна быть длиннее 320 символов.")]
        [Display(Name = "Электронная почта")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Признак того, что заказ является подарком.
        /// </summary>
        [Display(Name = "Это подарок")]
        public bool IsGift { get; set; }

        /// <summary>
        /// Имя получателя для подарка.
        /// </summary>
        [Display(Name = "Имя получателя")]
        [StringLength(200, ErrorMessage = "Имя получателя должно быть короче 200 символов.")]
        public string? RecipientName { get; set; }

        /// <summary>
        /// Телефон получателя для подарка.
        /// </summary>
        [Display(Name = "Телефон получателя")]
        [StringLength(40, ErrorMessage = "Телефон получателя не должен быть длиннее 40 символов.")]
        public string? RecipientPhone { get; set; }

        /// <summary>
        /// Город получателя.
        /// </summary>
        [Required(ErrorMessage = "Укажите город получателя.")]
        [StringLength(200, ErrorMessage = "Название города слишком длинное.")]
        [Display(Name = "Город получателя")]
        public string RecipientCity { get; set; } = string.Empty;

        /// <summary>
        /// Выбранный пункт выдачи.
        /// </summary>
        [Display(Name = "Пункт выдачи")]
        public int PickupPointId { get; set; }

        /// <summary>
        /// Текст открытки.
        /// </summary>
        [StringLength(300, ErrorMessage = "Текст открытки не должен быть длиннее 300 символов.")]
        [Display(Name = "Текст открытки")]
        public string? GiftMessage { get; set; }

        /// <summary>
        /// Выбранная волна.
        /// </summary>
        [Display(Name = "Волна доставки")]
        public int WaveId { get; set; }

        /// <summary>
        /// Термоупаковка, выбранная покупателем.
        /// </summary>
        [Display(Name = "Термоупаковка")]
        public bool ThermoPackRequested { get; set; }

        /// <summary>
        /// Промокод.
        /// </summary>
        [StringLength(32, ErrorMessage = "Промокод не должен быть длиннее 32 символов.")]
        [Display(Name = "Промокод")]
        public string? PromoCode { get; set; }
    }
}