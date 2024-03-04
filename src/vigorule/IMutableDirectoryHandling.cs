using JetBrains.Annotations;

namespace vigorule;

[PublicAPI]
public interface IMutableDirectoryHandling : IMutableHandling
{
    DirectoryInfo SourceDirectory { get; }
    bool KeepEmptyDirectory { get; }
    IFinalDirectoryHandling CheckAndTransform();
}