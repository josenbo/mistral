using Serilog.Events;
using vigobase;

namespace vigo;

internal record AppConfigRepoCheck(
    DirectoryInfo RepositoryRoot, 
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
    public override CommandEnum Command => CommandEnum.Check;
}