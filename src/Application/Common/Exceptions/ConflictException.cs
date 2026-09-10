namespace WareStockApi.Application.Common.Exceptions;

/// <summary>
/// Thrown when a request conflicts with the current state of a resource (HTTP 409),
/// e.g. registering with an email address that is already in use.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException()
        : base()
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
