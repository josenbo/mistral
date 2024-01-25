using Serilog.Events;

namespace vigo;

internal record Configuration(
    DirectoryInfo RepositoryRoot, 
    FileInfo Tarball, 
    string DeploymentConfigFileName,
    string? AdditionalTarRootFolder,
    FileInfo? Logfile,
    LogEventLevel LogLevel
    );