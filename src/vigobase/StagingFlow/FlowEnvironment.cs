using Ardalis.GuardClauses;
using JetBrains.Annotations;

namespace vigobase;

[PublicAPI]
public record FlowEnvironment(DirectoryInfo RepositoryRoot, FileInfo Tarball)
{
    public DirectoryInfo RepositoryRoot { get; } =
        Guard.Against.InvalidInput(RepositoryRoot, nameof(RepositoryRoot), IsValidRepositoryRoot);

    public FileInfo Tarball { get; } =
        Guard.Against.InvalidInput(Tarball, nameof(Tarball), IsValidTarballParameter);
    
    private static bool IsValidRepositoryRoot(DirectoryInfo repositoryRoot)
    {
        repositoryRoot.Refresh();
        return repositoryRoot.Exists && Path.IsPathRooted(repositoryRoot.FullName);
    }
    
    private static bool IsValidTarballParameter(FileInfo tarball)
    {
        tarball.Refresh();
        if (tarball.Directory is null || !tarball.Directory.Exists) return false;
        if (tarball.Exists)
            tarball.Delete();
        return true;
    }
}