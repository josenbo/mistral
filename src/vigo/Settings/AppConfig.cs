using Serilog.Events;
using vigobase;

namespace vigo;

internal abstract record AppConfig(
    DirectoryInfo TemporaryDirectory,
    FileInfo? Logfile,
    LogEventLevel LogLevel)
{
    public abstract CommandEnum Command { get; }
    
    public string GetTemporaryFilePath()
    {
        return Path.Combine(TemporaryDirectory.FullName, $"tempfile_{_tempFileSequence++}");
    }

    private int _tempFileSequence = Random.Shared.Next(100000000, 999999999);
}