namespace vigoscope;

internal class NameParserException : Exception
{
    public NameParserException()
    {
    }

    public NameParserException(string? message) : base(message)
    {
    }

    public NameParserException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}