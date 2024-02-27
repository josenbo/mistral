using Serilog.Events;
using vigobase;

namespace vigo;

internal record AppConfigFolderExplain(
    FileInfo FolderConfiguration, 
    DirectoryInfo TemporaryDirectory,
    FileInfo? Logfile,
    LogEventLevel LogLevel
) : AppConfig(
    TemporaryDirectory,
    Logfile,
    LogLevel
)
{
    public override CommandEnum Command => CommandEnum.Explain;
}