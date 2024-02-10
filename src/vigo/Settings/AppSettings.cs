using Serilog.Events;
using vigobase;

namespace vigo;

internal abstract record AppSettings(
    DirectoryInfo RepositoryRoot,
    string DeploymentConfigFileName,
    DirectoryInfo TemporaryDirectory,
    FileInfo? Logfile,
    LogEventLevel LogLevel
) : IAppSettings
{
    public FileHandlingParameters DefaultFileHandlingParams
    {
        get => _defaultFileHandlingParams ?? throw new NullReferenceException($"{nameof(_defaultFileHandlingParams)} is null");
        set => _defaultFileHandlingParams = value;
    }
    public string TemporaryTarballPath => Path.Combine(TemporaryDirectory.FullName, "vigo.tar.gz");
    public abstract CommandEnum Command { get; }
    public virtual bool IsDeploymentRun => Command == CommandEnum.DeployToTarball;
    public virtual bool IsCommitCheck => Command == CommandEnum.CheckCommit;
    
    public string GetRepoRelativePath(string path)
    {
        return Path.GetRelativePath(RepositoryRoot.FullName, path);
    }
    public string GetRepoRelativePath(FileSystemInfo file)
    {
        return Path.GetRelativePath(RepositoryRoot.FullName, file.FullName);
    }

    private FileHandlingParameters? _defaultFileHandlingParams;
}