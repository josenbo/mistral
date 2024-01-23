namespace vigobase.Exceptions;

public class VigoFatalException : VigoException
{
    public VigoFatalException()
    {
    }

    public VigoFatalException(string? message) : base(message)
    {
    }

    public VigoFatalException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}