using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Services.Catalog;

namespace SweetWave.Web.Pages.Admin.Products;

/// <summary>
/// Creation of a catalogue product.
/// </summary>
/// <param name="productManagementService">Product management service.</param>
public sealed class CreateModel(IProductManagementService productManagementService) : PageModel
{
    /// <summary>
    /// Data of the product form.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Creates the product and returns to the list.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect to the product list.</returns>
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
    /// Data of the product form with Russian validation messages.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Product identifier, for example <c>YKT-011</c>.
        /// </summary>
        [Required(ErrorMessage = "Укажите идентификатор товара.")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Идентификатор должен быть от 3 до 32 символов.")]
        [Display(Name = "Идентификатор")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Region of the manufacturer.
        /// </summary>
        [Display(Name = "Регион производителя")]
        public Region Region { get; set; } = Region.Yakutia;

        /// <summary>
        /// Manufacturer name.
        /// </summary>
        [Required(ErrorMessage = "Укажите производителя.")]
        [StringLength(200, ErrorMessage = "Название производителя слишком длинное.")]
        [Display(Name = "Производитель")]
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// Product name.
        /// </summary>
        [Required(ErrorMessage = "Укажите наименование товара.")]
        [StringLength(200, ErrorMessage = "Наименование товара слишком длинное.")]
        [Display(Name = "Наименование")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Catalogue category.
        /// </summary>
        [Required(ErrorMessage = "Укажите категорию.")]
        [StringLength(100, ErrorMessage = "Название категории слишком длинное.")]
        [Display(Name = "Категория")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Weight of a single unit, in grams.
        /// </summary>
        [Range(1, 100_000, ErrorMessage = "Вес должен быть от 1 до 100 000 граммов.")]
        [Display(Name = "Вес, г")]
        public int WeightGrams { get; set; } = 100;

        /// <summary>
        /// Price of a single unit, in rubles.
        /// </summary>
        [Range(0, 1_000_000, ErrorMessage = "Цена должна быть от 0 до 1 000 000 рублей.")]
        [Display(Name = "Цена, ₽")]
        public decimal Price { get; set; }

        /// <summary>
        /// Temperature sensitive flag.
        /// </summary>
        [Display(Name = "Боится мороза и жары")]
        public bool IsTemperatureSensitive { get; set; }

        /// <summary>
        /// Shelf life, in days.
        /// </summary>
        [Range(1, 3650, ErrorMessage = "Срок годности должен быть от 1 до 3650 дней.")]
        [Display(Name = "Срок годности, дней")]
        public int ShelfLifeDays { get; set; } = 180;

        /// <summary>
        /// Photo file name inside <c>wwwroot/images</c>.
        /// </summary>
        [StringLength(200, ErrorMessage = "Имя файла слишком длинное.")]
        [Display(Name = "Файл изображения")]
        public string? Photo { get; set; }

        /// <summary>
        /// Product description.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Описание не должно быть длиннее 1000 символов.")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        /// <summary>
        /// Converts the form data into a service command.
        /// </summary>
        /// <returns>Command for the product management service.</returns>
        public ProductInput ToProductInput() =>
            new(Id, Region, Manufacturer, Name, Category, WeightGrams, Price, IsTemperatureSensitive, ShelfLifeDays, Photo, Description);
    }
}