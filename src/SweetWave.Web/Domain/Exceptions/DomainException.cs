namespace SweetWave.Web.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule of the domain model is violated.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="message">Human readable description of the violated rule (in Russian).</param>
    public DomainException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">Human readable description of the violated rule (in Russian).</param>
    /// <param name="innerException">Exception that caused the rule violation.</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}