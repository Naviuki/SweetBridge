using System.Globalization;
using System.Text;
using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.ValueObjects;

/// <summary>
/// Нормализованный номер телефона в формате <c>+7XXXXXXXXXX</c>.
/// </summary>
public readonly struct PhoneNumber
{
    private const int NationalDigits = 10;
    private const string Prefix = "+7";
    private const string Hint = "Укажите телефон в формате +7 999 000-11-22.";

    private PhoneNumber(string value) => Value = value;

    /// <summary>
    /// Нормализованное значение номера.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Приводит произвольный ввод телефона к нормализованному виду.
    /// </summary>
    /// <param name="raw">Исходное значение.</param>
    /// <returns>Нормализованный номер.</returns>
    /// <exception cref="DomainException">Номер телефона некорректен.</exception>
    public static PhoneNumber Parse(string? raw)
    {
        var digits = new StringBuilder(NationalDigits + 1);
        foreach (char symbol in raw ?? string.Empty)
        {
            if (char.IsAsciiDigit(symbol))
            {
                digits.Append(symbol);
            }
        }

        ReadOnlySpan<char> sequence = digits.ToString().AsSpan();

        string national = sequence switch
        {
            _ when sequence.Length == NationalDigits => sequence.ToString(),
            _ when sequence.Length == NationalDigits + 1 && sequence[0] == '7' => sequence[1..].ToString(),
            _ when sequence.Length == NationalDigits + 1 && sequence[0] == '8' => sequence[1..].ToString(),
            _ => throw new DomainException(Hint)
        };

        if (!national.All(char.IsAsciiDigit))
        {
            throw new DomainException(Hint);
        }

        return new PhoneNumber(Prefix + national);
    }

    /// <summary>
    /// Возвращает нормализованный номер или <see langword="null"/>, если ввод некорректен.
    /// </summary>
    /// <param name="raw">Исходное значение.</param>
    /// <returns>Номер или <see langword="null"/>.</returns>
    public static string? TryNormalize(string? raw)
    {
        try
        {
            return Parse(raw).Value;
        }
        catch (DomainException)
        {
            return null;
        }
    }

    /// <summary>
    /// Форматирует номер для показа: <c>+7 999 000-11-22</c>.
    /// </summary>
    /// <returns>Отформатированный номер.</returns>
    public string ToDisplay()
    {
        if (Value.Length != Prefix.Length + NationalDigits)
        {
            return Value;
        }

        ReadOnlySpan<char> digits = Value.AsSpan(Prefix.Length);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"+7 {digits[..3]} {digits[3..6]}-{digits[6..8]}-{digits[8..10]}");
    }

    /// <summary>
    /// Возвращает значение по умолчанию.
    /// </summary>
    public static PhoneNumber Empty => new(string.Empty);

    /// <inheritdoc />
    public override string ToString() => Value;
}