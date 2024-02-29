using JetBrains.Annotations;
using vigobase;

namespace vigoconfig;

[PublicAPI]
internal class VigoParseFolderConfigException : VigoRecoverableException
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