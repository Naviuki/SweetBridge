using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Catalog;

namespace SweetBridge.Web.Pages.Admin.Products;

/// <summary>
/// Создание товара каталога.
/// </summary>
/// <param name="productManagementService">Сервис управления товарами.</param>
public sealed class CreateModel(IProductManagementService productManagementService) : PageModel
{
    /// <summary>
    /// Данные формы товара.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Создаёт товар и возвращает к списку.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Возврат к списку товаров.</returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            ProductSummary product = await productManagementService.CreateAsync(Input.ToProductInput(), cancellationToken);
            TempData["Success"] = $"Товар {product.Id} добавлен в каталог.";
        }
        catch (DomainException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }

        return RedirectToPage("/Admin/Products/Index");
    }

    /// <summary>
    /// Данные формы товара с русскими сообщениями валидации.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Идентификатор товара, например <c>YKT-011</c>.
        /// </summary>
        [Required(ErrorMessage = "Укажите идентификатор товара.")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Идентификатор должен быть от 3 до 32 символов.")]
        [Display(Name = "Идентификатор")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Регион производителя.
        /// </summary>
        [Display(Name = "Регион производителя")]
        public Region Region { get; set; } = Region.Yakutia;

        /// <summary>
        /// Название производителя.
        /// </summary>
        [Required(ErrorMessage = "Укажите производителя.")]
        [StringLength(200, ErrorMessage = "Название производителя слишком длинное.")]
        [Display(Name = "Производитель")]
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// Наименование товара.
        /// </summary>
        [Required(ErrorMessage = "Укажите наименование товара.")]
        [StringLength(200, ErrorMessage = "Наименование товара слишком длинное.")]
        [Display(Name = "Наименование")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Категория в каталоге.
        /// </summary>
        [Required(ErrorMessage = "Укажите категорию.")]
        [StringLength(100, ErrorMessage = "Название категории слишком длинное.")]
        [Display(Name = "Категория")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Вес одной штуки в граммах.
        /// </summary>
        [Range(1, 100_000, ErrorMessage = "Вес должен быть от 1 до 100 000 граммов.")]
        [Display(Name = "Вес, г")]
        public int WeightGrams { get; set; } = 100;

        /// <summary>
        /// Цена одной штуки в рублях.
        /// </summary>
        [Range(0, 1_000_000, ErrorMessage = "Цена должна быть от 0 до 1 000 000 рублей.")]
        [Display(Name = "Цена, ₽")]
        public decimal Price { get; set; }

        /// <summary>
        /// Признак температурной чувствительности.
        /// </summary>
        [Display(Name = "Боится мороза и жары")]
        public bool IsTemperatureSensitive { get; set; }

        /// <summary>
        /// Срок годности в днях.
        /// </summary>
        [Range(1, 3650, ErrorMessage = "Срок годности должен быть от 1 до 3650 дней.")]
        [Display(Name = "Срок годности, дней")]
        public int ShelfLifeDays { get; set; } = 180;

        /// <summary>
        /// Имя файла изображения в каталоге <c>wwwroot/images</c>.
        /// </summary>
        [StringLength(200, ErrorMessage = "Имя файла слишком длинное.")]
        [Display(Name = "Файл изображения")]
        public string? Photo { get; set; }

        /// <summary>
        /// Описание товара.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Описание не должно быть длиннее 1000 символов.")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        /// <summary>
        /// Преобразует данные формы в команду сервиса.
        /// </summary>
        /// <returns>Команда для сервиса управления товарами.</returns>
        public ProductInput ToProductInput() =>
            new(Id, Region, Manufacturer, Name, Category, WeightGrams, Price, IsTemperatureSensitive, ShelfLifeDays, Photo, Description);
    }
}
