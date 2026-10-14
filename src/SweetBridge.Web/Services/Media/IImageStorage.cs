using Microsoft.AspNetCore.Http;

namespace SweetBridge.Web.Services.Media;

/// <summary>
/// Результат проверки загруженного изображения.
/// </summary>
/// <param name="IsValid">Признак корректности файла.</param>
/// <param name="ErrorMessage">Сообщение об ошибке.</param>
public sealed record ImageValidationResult(bool IsValid, string? ErrorMessage)
{
    /// <summary>
    /// Создаёт успешный результат.
    /// </summary>
    /// <returns>Успешный результат.</returns>
    public static ImageValidationResult Ok() => new(true, null);

    /// <summary>
    /// Создаёт результат с ошибкой.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    /// <returns>Результат с ошибкой.</returns>
    public static ImageValidationResult Fail(string message) => new(false, message);
}

/// <summary>
/// Приём и проверка изображений товаров.
/// </summary>
public interface IImageStorage
{
    /// <summary>
    /// Проверяет сигнатуру и размер файла.
    /// </summary>
    /// <param name="file">Загруженный файл.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат проверки.</returns>
    Task<ImageValidationResult> ValidateAsync(IFormFile file, CancellationToken cancellationToken);

    /// <summary>
    /// Сохраняет изображение под сгенерированным именем.
    /// </summary>
    /// <param name="file">Загруженный файл.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Путь к файлу внутри uploads или <see langword="null"/>, если файл некорректен.</returns>
    Task<string?> SaveAsync(IFormFile file, CancellationToken cancellationToken);
}