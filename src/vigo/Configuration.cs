using Serilog.Events;

namespace vigo;

internal abstract record Configuration(
    DirectoryInfo RepositoryRoot,
    string DeploymentConfigFileName,
    FileInfo? Logfile,
    LogEventLevel LogLevel
)
{
    public abstract CommandEnum Command { get; }
}