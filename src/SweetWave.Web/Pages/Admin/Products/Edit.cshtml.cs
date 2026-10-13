using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SweetWave.Web.Domain.Enums;
using SweetWave.Web.Domain.Exceptions;
using SweetWave.Web.Services.Catalog;

namespace SweetWave.Web.Pages.Admin.Products;

/// <summary>
/// Правка товара каталога.
/// </summary>
/// <param name="productManagementService">Сервис управления товарами.</param>
public sealed class EditModel(IProductManagementService productManagementService) : PageModel
{
    /// <summary>
    /// Идентификатор редактируемого товара.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Данные формы товара.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Загружает товар в форму.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Задача, отражающая выполнение операции.</returns>
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        ProductSummary? product = await FindProductAsync(cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        Input = InputModel.FromProduct(product);

        return Page();
    }

    /// <summary>
    /// Обновляет товар и возвращает к списку.
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
            await productManagementService.UpdateAsync(Id, Input.ToProductInput(Id), cancellationToken);
            TempData["Success"] = $"Товар {Id} обновлён.";
        }
        catch (DomainException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }

        return RedirectToPage("/Admin/Products/Index");
    }

    private async Task<ProductSummary?> FindProductAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<ProductSummary> products = await productManagementService.GetAllAsync(cancellationToken);

        return products.FirstOrDefault(product => product.Id == Id);
    }

    /// <summary>
    /// Данные формы товара с русскими сообщениями валидации.
    /// </summary>
    public sealed class InputModel
    {
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
        public int WeightGrams { get; set; }

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
        public int ShelfLifeDays { get; set; }

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
        /// Заполняет форму данными товара.
        /// </summary>
        /// <param name="product">Редактируемый товар.</param>
        /// <returns>Данные формы.</returns>
        public static InputModel FromProduct(ProductSummary product)
        {
            ArgumentNullException.ThrowIfNull(product);

            return new InputModel
            {
                Region = product.Region,
                Manufacturer = product.Manufacturer,
                Name = product.Name,
                Category = product.Category,
                WeightGrams = product.WeightGrams,
                Price = product.Price,
                IsTemperatureSensitive = product.IsTemperatureSensitive,
                ShelfLifeDays = product.ShelfLifeDays,
                Photo = product.Photo,
                Description = product.Description
            };
        }

        /// <summary>
        /// Преобразует данные формы в команду сервиса.
        /// </summary>
        /// <param name="Id">Идентификатор товара.</param>
        /// <returns>Команда для сервиса управления товарами.</returns>
        public ProductInput ToProductInput(string id) =>
            new(id, Region, Manufacturer, Name, Category, WeightGrams, Price, IsTemperatureSensitive, ShelfLifeDays, Photo, Description);
    }
}
