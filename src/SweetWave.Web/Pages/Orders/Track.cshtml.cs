using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Services.Orders;

namespace SweetWave.Web.Pages.Orders;

/// <summary>
/// Order tracking by the order number and a phone number of the buyer or the recipient.
/// </summary>
/// <param name="orderService">Order service.</param>
public sealed class TrackModel(IOrderService orderService) : PageModel
{
    /// <summary>
    /// Search form.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Found order.
    /// </summary>
    public OrderSummary? Order { get; private set; }

    /// <summary>
    /// Indicates that the search did not find anything.
    /// </summary>
    public bool IsNotFound { get; private set; }

    /// <summary>
    /// Searches the order.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
    public async Task OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return;
        }

        Order = await orderService.FindAsync(Input.Number, Input.Phone, cancellationToken);
        IsNotFound = Order is null;
    }

    /// <summary>
    /// Data of the tracking form.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Order number.
        /// </summary>
        [Required(ErrorMessage = "Укажите номер заказа.")]
        [StringLength(16, ErrorMessage = "Номер заказа выглядит неверно.")]
        [Display(Name = "Номер заказа")]
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Phone number of the buyer or the recipient.
        /// </summary>
        [Required(ErrorMessage = "Укажите телефон покупателя или получателя.")]
        [StringLength(40, ErrorMessage = "Телефон не должен быть длиннее 40 символов.")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;
    }
}