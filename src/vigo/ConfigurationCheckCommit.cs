using Serilog.Events;

namespace vigo;

internal record ConfigurationCheckCommit(
    DirectoryInfo RepositoryRoot, 
    string DeploymentConfigFileName,
    FileInfo? Logfile,
    LogEventLevel LogLevel
) : Configuration(
    RepositoryRoot, 
    DeploymentConfigFileName,
    Logfile,
    LogLevel
)
{
    public override CommandEnum Command => CommandEnum.CheckCommit;
}