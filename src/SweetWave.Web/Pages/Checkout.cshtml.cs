using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Services.Cart;
using SweetWave.Web.Services.Orders;
using SweetWave.Web.Services.Waves;

namespace SweetWave.Web.Pages;

/// <summary>
/// Оформление: данные покупателя и получателя, выбор волны с прозрачным расчётом доставки.
/// </summary>
/// <param name="cartService">Сервис корзины.</param>
/// <param name="waveService">Сервис волн.</param>
/// <param name="orderService">Сервис заказов.</param>
public sealed class CheckoutModel(
    ICartService cartService,
    IWaveService waveService,
    IOrderService orderService) : PageModel
{
    /// <summary>
    /// Данные формы оформления заказа.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Текущая корзина.
    /// </summary>
    public CartView Cart { get; private set; } = new([]);

    /// <summary>
    /// Волны, которые ещё принимают заказы для корзины.
    /// </summary>
    public IReadOnlyList<WaveOffer> Offers { get; private set; } = [];

    /// <summary>
    /// Загружает корзину и рассчитывает предложения открытых волн.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Cart = await cartService.GetCartAsync(cancellationToken);

        if (Cart.IsEmpty)
        {
            return RedirectToPage("/Cart");
        }

        await LoadOffersAsync(cancellationToken);

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
            return RedirectToPage("/Cart");
        }

        await LoadOffersAsync(cancellationToken);

        if (Input.WaveId <= 0)
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.WaveId)}", "Выберите волну доставки.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        PlaceOrderCommand command = new(
            Input.WaveId,
            Input.BuyerName,
            Input.BuyerPhone,
            Input.IsGift,
            Input.IsGift ? Input.RecipientName : null,
            Input.IsGift ? Input.RecipientPhone : null,
            Input.RecipientCity,
            Input.RecipientPickupPoint,
            Input.GiftMessage,
            Input.ThermoPackRequested,
            [.. Cart.Lines.Select(line => new OrderLineRequest(line.Product.Id, line.Quantity))]);

        try
        {
            OrderSummary order = await orderService.PlaceAsync(command, cancellationToken);

            cartService.Clear();
            TempData["Success"] = "Заказ оформлен. Стоимость зафиксирована на момент оформления.";

            return RedirectToPage("/Orders/Confirmation", new { number = order.Number });
        }
        catch (DomainException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadOffersAsync(cancellationToken);

            return Page();
        }
    }

    private async Task LoadOffersAsync(CancellationToken cancellationToken)
    {
        ShippingDirection direction = Cart.Direction ?? ShippingDirection.YakutiaToDonbass;

        Offers = await waveService.GetOpenOffersAsync(
            direction,
            Cart.Products,
            Cart.TotalWeightGrams,
            Input.ThermoPackRequested,
            cancellationToken);
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
        /// Пункт выдачи службы доставки.
        /// </summary>
        [StringLength(200, ErrorMessage = "Название пункта выдачи слишком длинное.")]
        [Display(Name = "Пункт выдачи")]
        public string? RecipientPickupPoint { get; set; }

        /// <summary>
        /// Текст открытки.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Текст открытки не должен быть длиннее 1000 символов.")]
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
    }
}
