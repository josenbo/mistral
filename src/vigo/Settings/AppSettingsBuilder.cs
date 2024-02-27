using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Serilog.Events;
using vigobase;

namespace vigo;

internal class AppSettingsBuilder
{
    #region instance members

    private AppConfigRepo LocalAppConfigRepo { get; }
    
    private AppSettingsBuilder()
    {
        try
        {
            var command = GetCommand();

            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            LocalAppConfigRepo = command switch
            {
                CommandEnum.Deploy => new AppConfigRepoDeploy(
                    RepositoryRoot: GetRepositoryRoot(),
                    Tarball: GetTarballFile()
                    ),
                CommandEnum.Check => new AppConfigRepoCheck(
                    RepositoryRoot: GetRepositoryRoot()
                    ),
                _ => throw new ArgumentException($"The command {command} is not handled", nameof(command))
            };
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(AppSettingsBuilder)} constructor. Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
    }
    
    #endregion

    #region static members

    // "Tarball" or "Check" with case-insensitive comparison 
    private const string EnvVarVigoCommand = "VIGO_COMMAND";
    // Directory must exist 
    private const string EnvVarVigoRepositoryRoot = "VIGO_REPOSITORY_ROOT";
    // Directory must exist, file will be created or overwritten
    private const string EnvVarVigoTarballFile = "VIGO_TARBALL_FILE";
    // Directory must exist, file will be created or written to
    private const string EnvVarVigoLogfile = "VIGO_LOGFILE";
    // A log level for logging to a file only (Fatal, Error, Warning, *Information*, Debug) case-insensitive
    private const string EnvVarVigoLogLevel = "VIGO_LOGLEVEL";

    public static bool TryGetSingleRunLogConfiguration(out LogEventLevel loglevel, [NotNullWhen(true)] out FileInfo? logfile)
    {
        loglevel = GetLogLevel();
        logfile = GetLogfile();
        return (logfile is not null);
    }
        
    public static AppConfigRepo AppConfigRepo => _appSettings ?? CreateAppSettings();

    private static AppConfigRepo CreateAppSettings()
    {
        lock (AppSettingsLock)
        {
            if (_appSettings is null)
            {
                var builder = new AppSettingsBuilder();
                _appSettings = builder.LocalAppConfigRepo;
            }

            return _appSettings;
        }
    }

    // public static FileHandlingParameters DefaultFileHandlingParams => _defaultFileHandlingParams ??= Singleton.DefaultHandling;
    // private static AppSettingsBuilder Singleton => _singleton ??= new AppSettingsBuilder();
    
    private static CommandEnum GetCommand()
    {
        try
        {
            var command = GetEnvironmentVariable(EnvVarVigoCommand);
        
            if (command is null)
            {
                const string message = $"Expected the command in the environment variable {EnvVarVigoCommand}";
                Console.Error.WriteLine(message);
                throw new VigoFatalException(message);
            }

            if (20 < command.Length)
                command = command[..20];

            command = command.Trim().ToLowerInvariant();

            return CommandEnumHelper.Parse(command);
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(AppSettingsBuilder)}.{nameof(GetRepositoryRoot)}(). Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
    }
    
    private static DirectoryInfo GetRepositoryRoot()
    {
        try
        {
            var path = GetEnvironmentVariable(EnvVarVigoRepositoryRoot);

            if (path is null)
            {
                const string message = $"Expected the repository root path in the environment variable {EnvVarVigoRepositoryRoot}";
                Console.Error.WriteLine(message);
                throw new VigoFatalException(message);
            }

            if (4096 < path.Length)
                path = path[..4096];

            if (!Directory.Exists(path))
            {
                const string message = $"Invalid repository root path in the environment variable {EnvVarVigoRepositoryRoot}";
                Console.Error.WriteLine(message);
                throw new VigoFatalException(message);
            }

            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(path);
            
            return new DirectoryInfo(path);
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(AppSettingsBuilder)}.{nameof(GetRepositoryRoot)}(). Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
    }
    
    private static FileInfo GetTarballFile()
    {
        try
        {
            var path = GetEnvironmentVariable(EnvVarVigoTarballFile);
        
            if (path is null)
            {
                const string message = $"Expected the tarball file path in the environment variable {EnvVarVigoTarballFile}";
                Console.Error.WriteLine(message);
                throw new VigoFatalException(message);
            }

            if (4096 < path.Length)
                path = path[..4096];

            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(path);
            
            var fileInfo = new FileInfo(path);
        
            if (fileInfo.Directory is null || !fileInfo.Directory.Exists) 
            {
                var message = $"Expected the tarball file path to be located in an existing directory (Tarball file path: {path})";
                Console.Error.WriteLine(message);
                throw new VigoFatalException(message);
            }

            if (!fileInfo.Name.EndsWith(".tar.gz") || fileInfo.Name.Length < 8) 
            {
                var message = $"Expected the tarball file name to have a .tar.gz suffix and a non-empty base name (Tarball file name: {fileInfo.Name})";
                Console.Error.WriteLine(message);
                throw new VigoFatalException(message);
            }
            
            if (fileInfo.Exists)
                fileInfo.Delete();
        
            return fileInfo;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(AppSettingsBuilder)}.{nameof(GetTarballFile)}(). Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
    }
    
    private static FileInfo? GetLogfile()
    {
        try
        {
        
            var path = GetEnvironmentVariable(EnvVarVigoLogfile);

            if (path is null)
                return null;

            if (4096 < path.Length)
                path = path[..4096];
        
            var fileInfo = new FileInfo(path);

            if (fileInfo.Directory is not null && fileInfo.Directory.Exists)
            {
                if (!fileInfo.Exists) 
                    return fileInfo;
                fileInfo.Delete();
                fileInfo.Refresh();
                return fileInfo;
            }
        
            Console.Error.WriteLine($"Warning: The logfile directory does mot exist");
            return null;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(AppSettingsBuilder)}.{nameof(GetLogfile)}(). Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
    }

    private static LogEventLevel GetLogLevel()
    {
        try
        {
            var value = GetEnvironmentVariable(EnvVarVigoLogLevel, "Information");

            if (string.IsNullOrWhiteSpace(value))
                return LogEventLevel.Information;
            
            if (40 < value.Length)
                value = value[..40];
        
            if (Enum.TryParse<LogEventLevel>(value, true, out var logLevel)) 
                return logLevel;
        
            logLevel = LogEventLevel.Information;

            Console.Error.WriteLine($"Warning: Invalid log level {value} specified. Will use {logLevel} instead");

            return logLevel;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(AppSettingsBuilder)}.{nameof(GetLogLevel)}(). Message was: {e.Message}");
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
 
    
    private static AppConfigRepo? _appSettings;
    private static readonly string AppSettingsLock = new string("AppSettings");

    #endregion
}