using System.Runtime.Serialization;

namespace vigoftg;

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