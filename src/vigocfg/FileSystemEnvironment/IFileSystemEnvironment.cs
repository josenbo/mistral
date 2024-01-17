namespace vigocfg;

public interface IFileSystemEnvironment
{
    DirectoryInfo SourceRepositoryRoot { get; }
    DirectoryInfo SourceRepositoryFlyway { get; }
    DirectoryInfo SourceRepositoryStammaus { get; }
    DirectoryInfo SourceRepositoryCupines { get; }
    DirectoryInfo? TargetFolderFlyway { get; }
    DirectoryInfo? TargetFolderStammaus { get; }
    DirectoryInfo? TargetFolderCupines { get; }
}