using Serilog.Events;
using vigobase;

namespace vigo;

internal record AppSettingsCheckCommit(
    DirectoryInfo RepositoryRoot, 
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
    public override CommandEnum Command => CommandEnum.CheckCommit;
}