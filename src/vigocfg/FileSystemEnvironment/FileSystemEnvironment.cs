namespace vigocfg;

internal class FileSystemEnvironment : IFileSystemEnvironment
{
    public DirectoryInfo SourceRepositoryRoot { get; }
    public DirectoryInfo SourceRepositoryFlyway { get; }
    public DirectoryInfo SourceRepositoryStammaus { get; }
    public DirectoryInfo SourceRepositoryCupines { get; }
    public DirectoryInfo? TargetFolderFlyway { get; }
    public DirectoryInfo? TargetFolderStammaus { get; }
    public DirectoryInfo? TargetFolderCupines { get; }

    internal FileSystemEnvironment(Configuration configuration)
    {
        SourceRepositoryRoot = configuration.SourceRepositoryRoot;
        TargetFolderFlyway = configuration.TargetFolderFlyway;
        TargetFolderCupines = configuration.TargetFolderCupines;
        TargetFolderStammaus = configuration.TargetFolderStammaus;

        if (!SourceRepositoryRoot.Exists)
            throw new DirectoryNotFoundException($"{nameof(SourceRepositoryRoot)} not found ({SourceRepositoryRoot.FullName})");
        if (TargetFolderFlyway is not null && !TargetFolderFlyway.Exists)
            throw new DirectoryNotFoundException($"{nameof(TargetFolderFlyway)} not found ({TargetFolderFlyway.FullName})");
        if (TargetFolderCupines is not null && !TargetFolderCupines.Exists)
            throw new DirectoryNotFoundException($"{nameof(TargetFolderCupines)} not found ({TargetFolderCupines.FullName})");
        if (TargetFolderStammaus is not null && !TargetFolderStammaus.Exists)
            throw new DirectoryNotFoundException($"{nameof(TargetFolderStammaus)} not found ({TargetFolderStammaus.FullName})");

        SourceRepositoryFlyway = new DirectoryInfo(Path.Combine(
            SourceRepositoryRoot.FullName.PrependToStringArray(configuration.SourceRepositoryFlywaySubfolders)));
        SourceRepositoryCupines = new DirectoryInfo(Path.Combine(
            SourceRepositoryRoot.FullName.PrependToStringArray(configuration.SourceRepositoryCupinesSubfolders)));
        SourceRepositoryStammaus = new DirectoryInfo(Path.Combine(
            SourceRepositoryRoot.FullName.PrependToStringArray(configuration.SourceRepositoryStammausSubfolders)));
            
        if (!SourceRepositoryFlyway.Exists)
            throw new DirectoryNotFoundException($"{nameof(SourceRepositoryFlyway)} not found ({SourceRepositoryFlyway.FullName})");
        if (!SourceRepositoryCupines.Exists)
            throw new DirectoryNotFoundException($"{nameof(SourceRepositoryCupines)} not found ({SourceRepositoryCupines.FullName})");
        if (!SourceRepositoryStammaus.Exists)
            throw new DirectoryNotFoundException($"{nameof(SourceRepositoryStammaus)} not found ({SourceRepositoryStammaus.FullName})");
    }
}