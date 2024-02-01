using Serilog.Events;

namespace vigo;

internal record ConfigurationDeployToTarball(
    DirectoryInfo RepositoryRoot, 
    FileInfo Tarball, 
    string DeploymentConfigFileName,
    string? AdditionalTarRootFolder,
    DirectoryInfo TemporaryDirectory,
    FileInfo? Logfile,
    LogEventLevel LogLevel
) : Configuration(
    RepositoryRoot, 
    DeploymentConfigFileName,
    TemporaryDirectory,
    Logfile,
    LogLevel
)
{
    public override CommandEnum Command => CommandEnum.DeployToTarball;
}