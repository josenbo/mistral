using System.Globalization;
using Ardalis.GuardClauses;
using Serilog.Events;
using vigobase;

namespace vigo;

internal class AppSettingsBuilder
{
    #region instance members

    private AppSettings LocalAppSettings { get; }
    private FileHandlingParameters DefaultHandling { get; }

    private FileHandlingParameters FinalCatchAllHandling { get; }
    private FileHandlingParameters DeployConfigHandling { get; }
    
    private AppSettingsBuilder()
    {
        try
        {
            var command = GetCommand();

            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            LocalAppSettings = command switch
            {
                CommandEnum.DeployToTarball => new AppSettingsDeployToTarball(
                    RepositoryRoot: GetRepositoryRoot(),
                    Tarball: GetTarballFile(), 
                    TemporaryDirectory: GetTemporaryDirectory(),
                    Logfile: GetLogfile(),
                    LogLevel: GetLogLevel()),
                CommandEnum.CheckCommit => new AppSettingsCheckCommit(
                    RepositoryRoot: GetRepositoryRoot(),
                    TemporaryDirectory: GetTemporaryDirectory(),
                    Logfile: GetLogfile(),
                    LogLevel: GetLogLevel()),
                _ => throw new ArgumentException($"The command {command} is not handled", nameof(command))
            };
            
            var asciiGerman = ValidCharactersHelper.ParseConfiguration("AsciiGerman");
    
            DefaultHandling = new FileHandlingParameters(
                Settings: LocalAppSettings,
                StandardModeForFiles: (UnixFileMode)0b_110_110_100,
                StandardModeForDirectories: (UnixFileMode)0b_111_111_101,
                FileType: FileTypeEnum.BinaryFile, 
                SourceFileEncoding: FileEncodingEnum.UTF_8,
                TargetFileEncoding: FileEncodingEnum.UTF_8,
                LineEnding: LineEndingEnum.LF,
                Permissions: FilePermission.UseDefault, 
                FixTrailingNewline: true,
                ValidCharsRegex: asciiGerman,
                Targets: [ "Prod", "NonProd" ]
            );

            LocalAppSettings.DefaultFileHandlingParams = DefaultHandling;
            
            FinalCatchAllHandling = DefaultHandling with {
                FileType = FileTypeEnum.BinaryFile,
                Permissions = FilePermission.UseDefault
            };

            LocalAppSettings.FinalCatchAllRule = new StandardFileHandling(Array.Empty<ConfigurationFilename>(), FinalCatchAllHandling, false);

            DeployConfigHandling = DefaultHandling with
            {
                FileType = FileTypeEnum.BinaryFile,
                Permissions = FilePermission.UseDefault
            };

            var configurationFilenames = new List<ConfigurationFilename>()
            {
                new ConfigurationFilename("deployment-rules.md", ConfigurationFileTypeEnum.MarkdownFormat),
                new ConfigurationFilename("deployment-rules.vigo", ConfigurationFileTypeEnum.NativeFormat)
            };
            
            LocalAppSettings.DeployConfigRule = new StandardFileHandling(
                GetDeploymentConfigFileNames(configurationFilenames), 
                DeployConfigHandling, 
                false);
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
    // Filename to look for in each repository folder (and its default value)
    private const string EnvVarVigoDeployConfigFilenames = "VIGO_DEPLOY_CONFIG_FILENAMES";
    // The name of a virtual folder to become the root directory in the tarball
    private const string EnvVarVigoTempDir = "VIGO_TEMP_DIR";
    // Directory must exist, file will be created or written to
    private const string EnvVarVigoLogfile = "VIGO_LOGFILE";
    // A log level for logging to a file only (Fatal, Error, Warning, *Information*, Debug) case-insensitive
    private const string EnvVarVigoLogLevel = "VIGO_LOGLEVEL";

    public static AppSettings AppSettings => _appSettings ?? CreateAppSettings();

    private static AppSettings CreateAppSettings()
    {
        lock (AppSettingsLock)
        {
            if (_appSettings is null)
            {
                var builder = new AppSettingsBuilder();
                _appSettings = builder.LocalAppSettings;
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

    private static List<ConfigurationFilename> GetDeploymentConfigFileNames(IEnumerable<ConfigurationFilename> defaultFilenames)
    {
        try
        {
            var retval = new List<ConfigurationFilename>();
            var listOfFileNames = GetEnvironmentVariable(EnvVarVigoDeployConfigFilenames);

            if (listOfFileNames is null)
            {
                retval.AddRange(defaultFilenames);
            }
            else
            {
                if (256 < listOfFileNames.Length)
                    listOfFileNames = listOfFileNames[..256];

                foreach (var filename in listOfFileNames.Split(FilenameSeparators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    var extension = Path.GetExtension(filename);

                    switch (extension)
                    {
                        case ".md":
                            retval.Add(new ConfigurationFilename(filename, ConfigurationFileTypeEnum.MarkdownFormat));
                            break;
                        case ".vigo":
                            retval.Add(new ConfigurationFilename(filename, ConfigurationFileTypeEnum.NativeFormat));
                            break;
                        default:
                            throw new VigoFatalException($"Do not know how to handle the configuration file name {filename}");
                    }
                }
            }

            if (retval.Count == 0)
            {
                const string message = "There is no configuration filename to look for in repository folders";
                Console.Error.WriteLine(message);
                throw new VigoFatalException(message);
            }
            
            foreach (var configurationFilename in retval)
            {
                if (configurationFilename.FileName.Contains(Path.DirectorySeparatorChar) || configurationFilename.FileName.Contains(Path.AltDirectorySeparatorChar))
                {
                    const string message = "The deployment configuration file name is not allowed to contain path separators";
                    Console.Error.WriteLine(message);
                    throw new VigoFatalException(message);
                }
            
                // trigger exception on a file name containing illegal characters 
                File.Exists(configurationFilename.FileName);
            }

            return retval;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(AppSettingsBuilder)}.{nameof(GetDeploymentConfigFileNames)}(). Message was: {e.Message}");
            throw new VigoFatalException("Startup configuration failed", e);
        }
    }

    private static DirectoryInfo GetTemporaryDirectory()
    {
        try
        {
            var path = GetEnvironmentVariable(EnvVarVigoTempDir) ?? Path.GetTempPath();

            if (4096 < path.Length)
                path = path[..4096];

            if (!Directory.Exists(path))
            {
                const string message = "Could not locate the directory for temporary files";
                Console.Error.WriteLine(message);
                throw new VigoFatalException(message);
            }
                
            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(path);
            
            var formatProvider = CultureInfo.GetCultureInfo("en-US");
            // ReSharper disable StringLiteralTypo
            var timestamp = DateTime.Now.ToString("DyyyyMMdd_THHmmss", formatProvider);
            // ReSharper restore StringLiteralTypo
            var random = Random.Shared.Next(1,99999).ToString("00000", formatProvider);
            var name = $"VIGO_{timestamp}_R{random}";

            path = Path.Combine(path, name);
            
            var directoryInfo = new DirectoryInfo(path);

            if (directoryInfo.Exists)
            {
                const string message = "Could not set up the directory for temporary files";
                Console.Error.WriteLine(message);
                throw new VigoFatalException(message);
            }
            
            directoryInfo.Create();
            
            return directoryInfo;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Console.Error.WriteLine($"{e.GetType().Name} in startup configuration in {nameof(AppSettingsBuilder)}.{nameof(GetTemporaryDirectory)}(). Message was: {e.Message}");
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

            if (fileInfo.Directory is not null && fileInfo.Directory.Exists) return fileInfo;
        
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
 
    
    private static AppSettings? _appSettings;
    private static readonly string AppSettingsLock = new string("AppSettings");
    private static readonly char[] FilenameSeparators = new char[] { ',', ';', ' ' };

    #endregion
}