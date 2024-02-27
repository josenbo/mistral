using Serilog.Events;
using vigobase;

namespace vigo;

internal record AppConfigRepoDeploy(
    DirectoryInfo RepositoryRoot, 
    FileInfo Tarball, 
    DirectoryInfo TemporaryDirectory,
    FileInfo? Logfile,
    LogEventLevel LogLevel
) : AppConfigRepo(
    RepositoryRoot, 
    TemporaryDirectory,
    Logfile,
    LogLevel
)
{
    public override CommandEnum Command => CommandEnum.Deploy;
}