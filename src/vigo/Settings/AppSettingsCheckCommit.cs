using Serilog.Events;
using vigobase;

namespace vigo;

internal record AppSettingsCheckCommit(
    DirectoryInfo RepositoryRoot, 
    string DeploymentConfigFileName,
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
    public override CommandEnum Command => CommandEnum.CheckCommit;
}