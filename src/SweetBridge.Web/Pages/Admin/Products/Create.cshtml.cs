using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Media;

namespace SweetBridge.Web.Pages.Admin.Products;

/// <summary>
/// Создание товара с загрузкой фотографии.
/// </summary>
/// <param name="productService">Сервис управления каталогом.</param>
/// <param name="imageStorage">Хранилище изображений.</param>
public sealed class CreateModel(IProductManagementService productService, IImageStorage imageStorage) : PageModel
{
    /// <summary>
    /// Данные товара.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Загружаемое изображение товара.
    /// </summary>
    [BindProperty]
    public IFormFile? Photo { get; set; }

    /// <summary>
    /// Создаёт товар.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на список товаров.</returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string? photoPath = null;

        if (Photo is not null && Photo.Length > 0)
        {
            ImageValidationResult validation = await imageStorage.ValidateAsync(Photo, cancellationToken);

            if (!validation.IsValid)
            {
                ModelState.AddModelError(nameof(Photo), validation.ErrorMessage ?? "Файл не принят.");

                return Page();
            }

            photoPath = await imageStorage.SaveAsync(Photo, cancellationToken);
        }

        try
        {
            await productService.CreateAsync(ToInput(photoPath), cancellationToken);
            TempData["Success"] = "Товар создан.";

            return RedirectToPage("/Admin/Products/Index");
        }
        catch (DomainException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            return Page();
        }
    }

    private ProductInput ToInput(string? photoPath) =>
        new(
            Input.Id,
            Input.ProducerName,
            Input.Region,
            Input.Name,
            Input.Category,
            Input.WeightGrams,
            Input.Price,
            Input.IsTemperatureSensitive,
            Input.ShelfLifeDays,
            photoPath,
            Input.Description);

    /// <summary>
    /// Данные формы товара.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Идентификатор товара.
        /// </summary>
        [Required(ErrorMessage = "Укажите идентификатор товара.")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Идентификатор должен быть от 3 до 32 символов.")]
        [Display(Name = "Идентификатор")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Название производителя.
        /// </summary>
        [Required(ErrorMessage = "Укажите производителя.")]
        [StringLength(200, ErrorMessage = "Название производителя не должно быть длиннее 200 символов.")]
        [Display(Name = "Производитель")]
        public string ProducerName { get; set; } = string.Empty;

        /// <summary>
        /// Витрина.
        /// </summary>
        [Display(Name = "Витрина")]
        public Region Region { get; set; } = Region.Yakutia;

        /// <summary>
        /// Название товара.
        /// </summary>
        [Required(ErrorMessage = "Укажите название товара.")]
        [StringLength(200, ErrorMessage = "Название не должно быть длиннее 200 символов.")]
        [Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Категория.
        /// </summary>
        [Required(ErrorMessage = "Укажите категорию.")]
        [StringLength(100, ErrorMessage = "Категория не должна быть длиннее 100 символов.")]
        [Display(Name = "Категория")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Вес одной штуки в граммах.
        /// </summary>
        [Range(1, 100_000, ErrorMessage = "Вес должен быть от 1 до 100 000 граммов.")]
        [Display(Name = "Вес, г")]
        public int WeightGrams { get; set; } = 100;

        /// <summary>
        /// Цена в рублях.
        /// </summary>
        [Range(0, 1_000_000, ErrorMessage = "Цена должна быть от 0 до 1 000 000 рублей.")]
        [Display(Name = "Цена, ₽")]
        public decimal Price { get; set; }

        /// <summary>
        /// Признак термочувствительности.
        /// </summary>
        [Display(Name = "Термочувствительный")]
        public bool IsTemperatureSensitive { get; set; }

        /// <summary>
        /// Срок годности в днях.
        /// </summary>
        [Range(1, 3650, ErrorMessage = "Срок годности должен быть от 1 до 3650 дней.")]
        [Display(Name = "Срок годности, дней")]
        public int ShelfLifeDays { get; set; } = 30;

        /// <summary>
        /// Описание.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Описание не должно быть длиннее 1000 символов.")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }
    }
}