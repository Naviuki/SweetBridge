using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetBridge.Web.Domain.Entities;
using SweetBridge.Web.Domain.Enums;
using SweetBridge.Web.Domain.Exceptions;
using SweetBridge.Web.Services.Catalog;
using SweetBridge.Web.Services.Media;

namespace SweetBridge.Web.Pages.Admin.Products;

/// <summary>
/// Редактирование товара.
/// </summary>
/// <param name="productService">Сервис управления каталогом.</param>
/// <param name="imageStorage">Хранилище изображений.</param>
public sealed class EditModel(IProductManagementService productService, IImageStorage imageStorage) : PageModel
{
    /// <summary>
    /// Идентификатор товара.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Id { get; set; }

    /// <summary>
    /// Данные товара.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Новая фотография товара.
    /// </summary>
    [BindProperty]
    public IFormFile? Photo { get; set; }

    /// <summary>
    /// Текущий путь к фотографии.
    /// </summary>
    public string? CurrentPhotoPath { get; private set; }

    /// <summary>
    /// Загружает товар.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат выполнения.</returns>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        ProductSummary? product = await productService.GetAllAsync(cancellationToken) is var all
            ? all.FirstOrDefault(item => item.Id == Id)
            : null;

        if (product is null)
        {
            return NotFound();
        }

        CurrentPhotoPath = product.PhotoPath;
        Input = new InputModel
        {
            ProducerName = product.ProducerName,
            Region = product.Region,
            Name = product.Name,
            Category = product.Category,
            WeightGrams = product.WeightGrams,
            Price = product.Price,
            IsTemperatureSensitive = product.IsTemperatureSensitive,
            ShelfLifeDays = product.ShelfLifeDays,
            Description = product.Description
        };

        return Page();
    }

    /// <summary>
    /// Сохраняет изменения товара.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Перенаправление на список товаров.</returns>
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Id))
        {
            return Page();
        }

        string? photoPath = CurrentPhotoPath;

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
            ProductInput input = new(
                Id,
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

            await productService.UpdateAsync(Id, input, cancellationToken);
            TempData["Success"] = "Товар сохранён.";

            return RedirectToPage("/Admin/Products/Index");
        }
        catch (DomainException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            return Page();
        }
    }

    /// <summary>
    /// Данные формы товара.
    /// </summary>
    public sealed class InputModel
    {
        /// <summary>
        /// Название производителя.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Укажите производителя.")]
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Производитель")]
        public string ProducerName { get; set; } = string.Empty;

        /// <summary>
        /// Витрина.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Name = "Витрина")]
        public Region Region { get; set; } = Region.Yakutia;

        /// <summary>
        /// Название товара.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Укажите название товара.")]
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Название")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Категория.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Укажите категорию.")]
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Категория")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Вес одной штуки в граммах.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(1, 100_000, ErrorMessage = "Вес должен быть от 1 до 100 000 граммов.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Вес, г")]
        public int WeightGrams { get; set; }

        /// <summary>
        /// Цена в рублях.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(0, 1_000_000, ErrorMessage = "Цена должна быть от 0 до 1 000 000 рублей.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Цена, ₽")]
        public decimal Price { get; set; }

        /// <summary>
        /// Признак термочувствительности.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Display(Name = "Термочувствительный")]
        public bool IsTemperatureSensitive { get; set; }

        /// <summary>
        /// Срок годности в днях.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(1, 3650, ErrorMessage = "Срок годности должен быть от 1 до 3650 дней.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Срок годности, дней")]
        public int ShelfLifeDays { get; set; }

        /// <summary>
        /// Описание.
        /// </summary>
        [System.ComponentModel.DataAnnotations.StringLength(1000)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Описание")]
        public string? Description { get; set; }
    }
}