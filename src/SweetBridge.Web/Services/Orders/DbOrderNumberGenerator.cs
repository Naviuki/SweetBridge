using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SweetBridge.Web.Infrastructure.Persistence;

namespace SweetBridge.Web.Services.Orders;

/// <summary>
/// Генератор номеров заказов: криптографически случайный суффикс и проверка уникальности.
/// </summary>
public sealed class DbOrderNumberGenerator : IOrderNumberGenerator
{
    private const string Prefix = "SW-";
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int SuffixLength = 6;
    private const int MaxAttempts = 10;

    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="DbOrderNumberGenerator"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public DbOrderNumberGenerator(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<string> GenerateAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            string candidate = CreateCandidate();

            bool isUsed = await _dbContext.Orders.AnyAsync(order => order.Number == candidate, cancellationToken);

            if (!isUsed)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Не удалось сгенерировать уникальный номер заказа.");
    }

    private static string CreateCandidate()
    {
        char[] suffix = new char[SuffixLength];

        for (var index = 0; index < suffix.Length; index++)
        {
            suffix[index] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return string.Concat(Prefix, new string(suffix));
    }
}
