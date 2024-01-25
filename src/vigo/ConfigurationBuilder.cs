using Ardalis.GuardClauses;
using Serilog.Events;
using vigobase;

namespace vigo;

internal static class ConfigurationBuilder
{
    public static Configuration ActiveConfiguration => _activeConfiguration ??= BuildActiveConfiguration();

    private static Configuration BuildActiveConfiguration()
    {
        return new Configuration(
            RepositoryRoot: GetRepositoryRoot(),
            Tarball: GetTarballFile(),
            DeploymentConfigFileName: GetDeploymentConfigFileName(),
            AdditionalTarRootFolder: GetAdditionalTarRootFolder(),
            Logfile: GetLogfile(),
            LogLevel: GetLogLevel()
        );
    }

    private static DirectoryInfo GetRepositoryRoot()
    {
        const string environmentVariableName = "VIGO_REPOSITORY_ROOT";
        
        var path = GetEnvironmentVariable(environmentVariableName);
        
        if (path is null)
            throw new VigoFatalException(
                $"Expected the repository root path in the environment variable {environmentVariableName}");

        var directoryInfo = new DirectoryInfo(path);
        
        if (!directoryInfo.Exists || !Path.IsPathRooted(directoryInfo.FullName)) 
            throw new VigoFatalException(
                $"Expected the repository root path to be an existing and rooted path");

        return directoryInfo;
    }
    
    private static FileInfo GetTarballFile()
    {
        const string environmentVariableName = "VIGO_TARBALL_FILE";
        
        var path = GetEnvironmentVariable(environmentVariableName);
        
        if (path is null)
            throw new VigoFatalException(
                $"Expected the tarball file path in the environment variable {environmentVariableName}");

        var fileInfo = new FileInfo(path);
        
        if (fileInfo.Directory is null || !fileInfo.Directory.Exists) 
            throw new VigoFatalException(
                $"Expected the tarball file path to be located in an existing directory");

        if (fileInfo.Exists)
            fileInfo.Delete();
        
        return fileInfo;
    }

    private static string GetDeploymentConfigFileName()
    {
        const string environmentVariableName = "VIGO_DEPLOY_CONFIG_FILENAME";
        const string defaultDeploymentConfigFileName = "deployment.toml";
        
        return GetEnvironmentVariable(environmentVariableName, defaultDeploymentConfigFileName);
    }

    private static string? GetAdditionalTarRootFolder()
    {
        const string environmentVariableName = "VIGO_EXTRA_ROOT_FOLDER";
        
        return GetEnvironmentVariable(environmentVariableName);
    }

    private static FileInfo? GetLogfile()
    {
        const string environmentVariableName = "VIGO_LOGFILE";
        
        var path = GetEnvironmentVariable(environmentVariableName);

        if (path is null)
            return null;

        var fileInfo = new FileInfo(path[..4096]);

        if (fileInfo.Directory is not null && fileInfo.Directory.Exists) return fileInfo;
        
        Console.Error.WriteLine($"Warning: The logfile directory does mot exist");
        return null;
    }

    private static LogEventLevel GetLogLevel()
    {
        const string environmentVariableName = "VIGO_LOGLEVEL";
        
        var value = GetEnvironmentVariable(environmentVariableName, "Information")[..40];

        if (Enum.TryParse<LogEventLevel>(value, true, out var logLevel)) return logLevel;
        
        Console.Error.WriteLine($"Warning: Invalid log level {value} specified");
        logLevel = LogEventLevel.Information;

        return logLevel;
    }
    
    private static string GetEnvironmentVariable(string environmentVariableName, string defaultValue)
    {
        Guard.Against.NullOrWhiteSpace(defaultValue, nameof(defaultValue));
        var envValue = Environment.GetEnvironmentVariable(environmentVariableName);
        return string.IsNullOrWhiteSpace(envValue) ? defaultValue : envValue;
    }
    
    private static string? GetEnvironmentVariable(string environmentVariableName)
    {
        return Environment.GetEnvironmentVariable(environmentVariableName);
    }
    
    private static Configuration? _activeConfiguration = null;
}