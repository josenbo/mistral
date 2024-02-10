using Serilog.Events;
using vigobase;

namespace vigo;

internal record AppSettingsDeployToTarball(
    DirectoryInfo RepositoryRoot, 
    FileInfo Tarball, 
    string DeploymentConfigFileName,
    string? AdditionalTarRootFolder,
    DirectoryInfo TemporaryDirectory,
    FileInfo? Logfile,
    LogEventLevel LogLevel
) : AppSettings(
    RepositoryRoot, 
    DeploymentConfigFileName,
    TemporaryDirectory,
    Logfile,
    LogLevel
)
{
    public override CommandEnum Command => CommandEnum.DeployToTarball;
}