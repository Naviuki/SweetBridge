namespace SweetBridge.Web.Domain.Exceptions;

/// <summary>
/// Выбрасывается при нарушении бизнес-правила предметной области.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="DomainException"/>.
    /// </summary>
    /// <param name="message">Понятное пользователю описание нарушенного правила (на русском языке).</param>
    public DomainException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="DomainException"/> с внутренним исключением.
    /// </summary>
    /// <param name="message">Понятное пользователю описание нарушенного правила (на русском языке).</param>
    /// <param name="innerException">Исключение, вызвавшее нарушение правила.</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
