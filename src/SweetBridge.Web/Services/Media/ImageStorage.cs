using Microsoft.AspNetCore.Http;

namespace SweetBridge.Web.Services.Media;

/// <summary>
/// Хранилище изображений товаров: проверяет сигнатуру файла и сохраняет под именем GUID.
/// </summary>
public sealed class ImageStorage : IImageStorage
{
    /// <summary>
    /// Максимальный размер изображения в байтах.
    /// </summary>
    public const long MaxFileSizeBytes = 2 * 1024 * 1024;

    private const string RelativeDirectory = "uploads/products";
    private const int SignatureLength = 12;

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] WebpSignature = [0x52, 0x49, 0x46, 0x46];

    private readonly string _directory;
    private readonly ILogger<ImageStorage> _logger;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="ImageStorage"/>.
    /// </summary>
    /// <param name="environment">Среда выполнения: определяет корень wwwroot.</param>
    /// <param name="logger">Журнал событий.</param>
    public ImageStorage(IHostEnvironment environment, ILogger<ImageStorage> logger)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(logger);

        _directory = Path.Combine(environment.ContentRootPath, "wwwroot", "uploads", "products");
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ImageValidationResult> ValidateAsync(IFormFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Length == 0)
        {
            return ImageValidationResult.Fail("Файл изображения пуст.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return ImageValidationResult.Fail("Файл больше 2 МБ. Выберите изображение поменьше.");
        }

        var signature = new byte[SignatureLength];
        await using (Stream stream = file.OpenReadStream())
        {
            int read = await stream.ReadAtLeastAsync(signature, SignatureLength, throwOnEndOfStream: false, cancellationToken);

            if (read < SignatureLength)
            {
                return ImageValidationResult.Fail("Не удалось прочитать файл.");
            }
        }

        return DetectExtension(signature) is null
            ? ImageValidationResult.Fail("Поддерживаются только изображения PNG, JPEG и WEBP.")
            : ImageValidationResult.Ok();
    }

    /// <inheritdoc />
    public async Task<string?> SaveAsync(IFormFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        ImageValidationResult validation = await ValidateAsync(file, cancellationToken);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Изображение отклонено: {Reason}", validation.ErrorMessage);

            return null;
        }

        var signature = new byte[SignatureLength];
        await using (Stream stream = file.OpenReadStream())
        {
            await stream.ReadAtLeastAsync(signature, SignatureLength, throwOnEndOfStream: true, cancellationToken);
        }

        string extension = DetectExtension(signature) ?? ".png";
        string fileName = $"{Guid.NewGuid():N}{extension}";

        Directory.CreateDirectory(_directory);

        string path = Path.Combine(_directory, fileName);
        await using (FileStream target = File.Create(path))
        {
            await using Stream source = file.OpenReadStream();
            await source.CopyToAsync(target, cancellationToken);
        }

        return $"/{RelativeDirectory}/{fileName}";
    }

    private static string? DetectExtension(byte[] signature)
    {
        if (StartsWith(signature, PngSignature))
        {
            return ".png";
        }

        if (StartsWith(signature, JpegSignature))
        {
            return ".jpg";
        }

        if (StartsWith(signature, WebpSignature)
            && signature.Length >= 12
            && signature[8] == (byte)'W'
            && signature[9] == (byte)'E'
            && signature[10] == (byte)'B'
            && signature[11] == (byte)'P')
        {
            return ".webp";
        }

        return null;
    }

    private static bool StartsWith(byte[] data, byte[] prefix)
    {
        if (data.Length < prefix.Length)
        {
            return false;
        }

        for (int index = 0; index < prefix.Length; index++)
        {
            if (data[index] != prefix[index])
            {
                return false;
            }
        }

        return true;
    }
}