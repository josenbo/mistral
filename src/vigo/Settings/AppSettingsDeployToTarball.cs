using Serilog.Events;
using vigobase;

namespace vigo;

internal record AppSettingsDeployToTarball(
    DirectoryInfo RepositoryRoot, 
    FileInfo Tarball, 
    DirectoryInfo TemporaryDirectory,
    FileInfo? Logfile,
    LogEventLevel LogLevel
) : AppSettings(
    RepositoryRoot, 
    TemporaryDirectory,
    Logfile,
    LogLevel
)
{
    public override CommandEnum Command => CommandEnum.DeployToTarball;
}