namespace vigorule;

public interface IFinalDirectoryHandling : IFinalHandling
{
    DirectoryInfo SourceDirectory { get; }
    bool KeepEmptyDirectory { get; }
}