using Serilog.Events;
using vigobase;

namespace vigo;

internal abstract record AppSettings(
    DirectoryInfo RepositoryRoot,
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
    public StandardFileHandling DeployConfigRule
    {
        get => _deployConfigRule ?? throw new NullReferenceException($"{nameof(_deployConfigRule)} is null");
        set => _deployConfigRule = value;
    }
    public StandardFileHandling FinalCatchAllRule
    {
        get => _finalCatchAllRule ?? throw new NullReferenceException($"{nameof(_finalCatchAllRule)} is null");
        set => _finalCatchAllRule = value;
    }
    public string TemporaryTarballPath => Path.Combine(TemporaryDirectory.FullName, "vigo.tar.gz");
    public abstract CommandEnum Command { get; }

    public string GetRepoRelativePath(string path)
    {
        return Path.GetRelativePath(RepositoryRoot.FullName, path);
    }
    
    public string GetRepoRelativePath(FileSystemInfo file)
    {
        return Path.GetRelativePath(RepositoryRoot.FullName, file.FullName);
    }

    public string GetTemporaryFilePath()
    {
        return Path.Combine(TemporaryDirectory.FullName, $"tempfile_{_tempFileSequence++}");
    }

    private FileHandlingParameters? _defaultFileHandlingParams;
    private StandardFileHandling? _deployConfigRule;
    private StandardFileHandling? _finalCatchAllRule;
    private int _tempFileSequence = Random.Shared.Next(100000000, 999999999);
}