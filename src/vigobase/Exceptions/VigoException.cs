namespace vigobase.Exceptions;

public abstract class VigoException : Exception
{
    protected VigoException()
    {
    }

    protected VigoException(string? message) : base(message)
    {
    }

    protected VigoException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}