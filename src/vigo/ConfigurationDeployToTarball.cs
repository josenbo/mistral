using Serilog.Events;

namespace vigo;

internal record ConfigurationDeployToTarball(
    DirectoryInfo RepositoryRoot, 
    FileInfo Tarball, 
    string DeploymentConfigFileName,
    string? AdditionalTarRootFolder,
    FileInfo? Logfile,
    LogEventLevel LogLevel
) : Configuration(
    RepositoryRoot, 
    DeploymentConfigFileName,
    Logfile,
    LogLevel
)
{
    public override CommandEnum Command => CommandEnum.DeployToTarball;
}