namespace vigorule;

public interface IMutableDirectoryHandling : IMutableHandling
{
    DirectoryInfo SourceDirectory { get; }
    bool KeepEmptyDirectory { get; }
    IFinalDirectoryHandling CheckAndTransform();
}