using System.Text.RegularExpressions;
using SweetBridge.Web.Domain.Exceptions;

namespace SweetBridge.Web.Domain.ValueObjects;

/// <summary>
/// Номер заказа вида <c>SW-XXXXXX</c>.
/// </summary>
public readonly partial struct OrderNumber
{
    /// <summary>
    /// Префикс номера заказа.
    /// </summary>
    public const string Prefix = "SW-";

    private const int SuffixLength = 6;

    [GeneratedRegex("^SW-[A-Z0-9]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex OrderNumberRegex();

    private OrderNumber(string value) => Value = value;

    /// <summary>
    /// Значение номера.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Проверяет формат номера заказа.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <returns><see langword="true"/>, если формат корректен.</returns>
    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && OrderNumberRegex().IsMatch(value.Trim().ToUpperInvariant());

    /// <summary>
    /// Приводит ввод к верхнему регистру, если формат корректен.
    /// </summary>
    /// <param name="value">Исходное значение.</param>
    /// <returns>Номер заказа.</returns>
    /// <exception cref="DomainException">Формат номера некорректен.</exception>
    public static OrderNumber Parse(string? value)
    {
        string normalized = (value ?? string.Empty).Trim().ToUpperInvariant();

        if (!OrderNumberRegex().IsMatch(normalized))
        {
            throw new DomainException("Номер заказа указан в неверном формате. Ожидается SW-XXXXXX.");
        }

        return new OrderNumber(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}