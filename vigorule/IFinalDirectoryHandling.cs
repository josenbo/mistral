using JetBrains.Annotations;

namespace vigorule;

[PublicAPI]
public interface IFinalDirectoryHandling : IFinalHandling
{
    DirectoryInfo SourceDirectory { get; }
    bool KeepEmptyDirectory { get; }
}