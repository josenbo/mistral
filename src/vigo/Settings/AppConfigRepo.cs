using Serilog.Events;
using vigobase;

namespace vigo;

internal abstract record AppConfigRepo(
    DirectoryInfo RepositoryRoot,
    DirectoryInfo TemporaryDirectory,
    FileInfo? Logfile,
    LogEventLevel LogLevel
) : AppConfig(TemporaryDirectory, Logfile, LogLevel)
{
    public string TemporaryTarballPath => Path.Combine(TemporaryDirectory.FullName, "vigo.tar.gz");
}