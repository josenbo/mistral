using JetBrains.Annotations;

namespace vigoconfig;

[PublicAPI]
public class VigoParseFolderConfigException : Exception
{
    public VigoParseFolderConfigException()
    {
    }

    public VigoParseFolderConfigException(string? message) : base(message)
    {
    }

    public VigoParseFolderConfigException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}