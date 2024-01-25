using Ardalis.GuardClauses;
using Serilog.Events;
using vigobase;

namespace vigo;

internal static class ConfigurationBuilder
{
    public static Configuration ActiveConfiguration => _activeConfiguration ??= BuildActiveConfiguration();

    private static Configuration BuildActiveConfiguration()
    {
        try
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
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(ConfigurationBuilder)}.{nameof(BuildActiveConfiguration)}(). Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
    }

    private static DirectoryInfo GetRepositoryRoot()
    {
        try
        {
            const string environmentVariableName = "VIGO_REPOSITORY_ROOT";
        
            var path = GetEnvironmentVariable(environmentVariableName);
        
            if (path is null)
                throw new VigoFatalException(
                    $"Expected the repository root path in the environment variable {environmentVariableName}");

            if (4096 < path.Length)
                path = path[..4096];

            var directoryInfo = new DirectoryInfo(path);
        
            if (!directoryInfo.Exists || !Path.IsPathRooted(directoryInfo.FullName)) 
                throw new VigoFatalException(
                    $"Expected the repository root path to be an existing and rooted path");

            return directoryInfo;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(ConfigurationBuilder)}.{nameof(GetRepositoryRoot)}(). Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
    }
    
    private static FileInfo GetTarballFile()
    {
        try
        {
            const string environmentVariableName = "VIGO_TARBALL_FILE";
        
            var path = GetEnvironmentVariable(environmentVariableName);
        
            if (path is null)
                throw new VigoFatalException(
                    $"Expected the tarball file path in the environment variable {environmentVariableName}");

            if (4096 < path.Length)
                path = path[..4096];

            var fileInfo = new FileInfo(path);
        
            if (fileInfo.Directory is null || !fileInfo.Directory.Exists) 
                throw new VigoFatalException(
                    $"Expected the tarball file path to be located in an existing directory");

            if (fileInfo.Exists)
                fileInfo.Delete();
        
            return fileInfo;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(ConfigurationBuilder)}.{nameof(GetTarballFile)}(). Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
    }

    private static string GetDeploymentConfigFileName()
    {
        try
        {
            const string environmentVariableName = "VIGO_DEPLOY_CONFIG_FILENAME";
            const string defaultDeploymentConfigFileName = "deployment.toml";
        
            var fileName = GetEnvironmentVariable(environmentVariableName, defaultDeploymentConfigFileName);
        
            if (256 < fileName.Length)
                fileName = fileName[..256];
        
            return fileName;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(ConfigurationBuilder)}.{nameof(GetDeploymentConfigFileName)}(). Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
    }

    private static string? GetAdditionalTarRootFolder()
    {
        try
        {
            const string environmentVariableName = "VIGO_EXTRA_ROOT_FOLDER";
        
            var fileName = GetEnvironmentVariable(environmentVariableName);
        
            if (fileName is not null && 256 < fileName.Length)
                fileName = fileName[..256];
        
            return fileName;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(ConfigurationBuilder)}.{nameof(GetAdditionalTarRootFolder)}(). Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
    }

    private static FileInfo? GetLogfile()
    {
        try
        {
            const string environmentVariableName = "VIGO_LOGFILE";
        
            var path = GetEnvironmentVariable(environmentVariableName);

            if (path is null)
                return null;

            if (4096 < path.Length)
                path = path[..4096];
        
            var fileInfo = new FileInfo(path);

            if (fileInfo.Directory is not null && fileInfo.Directory.Exists) return fileInfo;
        
            Console.Error.WriteLine($"Warning: The logfile directory does mot exist");
            return null;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(ConfigurationBuilder)}.{nameof(GetLogfile)}(). Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
    }

    private static LogEventLevel GetLogLevel()
    {
        try
        {
            const string environmentVariableName = "VIGO_LOGLEVEL";
        
            var value = GetEnvironmentVariable(environmentVariableName, "Information");

            if (40 < value.Length)
                value = value[..40];
        
            if (Enum.TryParse<LogEventLevel>(value, true, out var logLevel)) return logLevel;
        
            Console.Error.WriteLine($"Warning: Invalid log level {value} specified");
            logLevel = LogEventLevel.Information;

            return logLevel;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(ConfigurationBuilder)}.{nameof(GetLogLevel)}(). Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
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