namespace vigo;

internal abstract record AppConfigFolder(DirectoryInfo BaseDirectory) : AppConfig;