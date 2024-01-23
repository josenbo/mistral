namespace vigobase.Exceptions;

public class VigoRecoverableException : VigoException
{
    public VigoRecoverableException()
    {
    }

    public VigoRecoverableException(string? message) : base(message)
    {
    }

    public VigoRecoverableException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}