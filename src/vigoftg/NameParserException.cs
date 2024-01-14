using System.Runtime.Serialization;

namespace vigoftg;

internal class NameParserException : Exception
{
    public NameParserException()
    {
    }

    protected NameParserException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public NameParserException(string? message) : base(message)
    {
    }

    public NameParserException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}