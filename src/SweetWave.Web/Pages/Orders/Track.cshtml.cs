using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Services.Orders;

namespace SweetWave.Web.Pages.Orders;

/// <summary>
/// Отслеживание заказа по номеру и телефону покупателя или получателя.
/// </summary>
/// <param name="orderService">Сервис заказов.</param>
public sealed class TrackModel(IOrderService orderService) : PageModel
{
    /// <summary>
    /// Форма поиска.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Найденный заказ.
    /// </summary>
    public OrderSummary? Order { get; private set; }

    /// <summary>
    /// Признак того, что поиск ничего не нашёл.
    /// </summary>
    public bool IsNotFound { get; private set; }

    /// <summary>
    /// Ищет заказ.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
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
    /// Данные формы отслеживания.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Номер заказа.
        /// </summary>
        [Required(ErrorMessage = "Укажите номер заказа.")]
        [StringLength(16, ErrorMessage = "Номер заказа выглядит неверно.")]
        [Display(Name = "Номер заказа")]
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Телефон покупателя или получателя.
        /// </summary>
        [Required(ErrorMessage = "Укажите телефон покупателя или получателя.")]
        [StringLength(40, ErrorMessage = "Телефон не должен быть длиннее 40 символов.")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;
    }
}
