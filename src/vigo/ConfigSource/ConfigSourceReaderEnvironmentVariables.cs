using System.Diagnostics.CodeAnalysis;
using Serilog;
using Serilog.Events;
using vigobase;

namespace vigo;

internal class ConfigSourceReaderEnvironmentVariables : IConfigSourceReader
{
    public EnvVar EnvVarSource { get; }
    public AppArguments Read(AppArguments initial)
    {
        var result = initial;

        if (result.RepositoryRoot is null)
            result = result with { RepositoryRoot = GetRepositoryRoot(EnvVarSource) };

        if (result.OutputFile is null) 
            result = result with { OutputFile = GetOutputFile(EnvVarSource) };
        
        if (result.Targets is null) 
            result = result with { Targets = GetTargets(EnvVarSource) };
        
        return result;
    }

    public ConfigSourceReaderEnvironmentVariables()
    {
        EnvVarSource = EnvVar.GetSystem();
    }
    
    public ConfigSourceReaderEnvironmentVariables(EnvVarMock mock)
    {
        EnvVarSource = mock;
    }
    
    // The top level directory of the repository (must exist) 
    private const string EnvVarVigoRepositoryRoot = "VIGO_REPOSITORY_ROOT";
    // The tarball to write (Directory must exist, file will be created or overwritten)
    private const string EnvVarVigoOutputFile = "VIGO_OUTPUT_FILE";
    // The build targets for the deploy action 
    private const string EnvVarVigoTargets = "VIGO_TARGETS";
    // Directory must exist, file will be created or written to
    private const string EnvVarVigoLogfile = "VIGO_LOG_FILE";
    // A log level for logging to a file only (Fatal, Error, Warning, *Information*, Debug) case-insensitive
    private const string EnvVarVigoLogLevel = "VIGO_LOG_LEVEL";

    public static bool TryGetSingleRunLogConfiguration(EnvVarMock mock, out LogEventLevel loglevel, [NotNullWhen(true)] out FileInfo? logfile)
    {
        loglevel = GetLogLevel(mock);
        logfile = GetLogfile(mock);
        return (logfile is not null);
    }
    
    public static bool TryGetSingleRunLogConfiguration(out LogEventLevel loglevel, [NotNullWhen(true)] out FileInfo? logfile)
    {
        var envVarSource = EnvVar.GetSystem();
        loglevel = GetLogLevel(envVarSource);
        logfile = GetLogfile(envVarSource);
        return (logfile is not null);
    }
    
    private static DirectoryInfo? GetRepositoryRoot(EnvVar envVarSource)
    {
        try
        {
            var path = envVarSource.GetEnvironmentVariable(EnvVarVigoRepositoryRoot);

            Log.Debug("Environment variable {TheName} has value {TheValue}", EnvVarVigoRepositoryRoot, path);
            
            if (path is null)
                return null;

            if (4096 < path.Length)
                path = path[..4096];

            if (!Directory.Exists(path))
                throw new VigoFatalException(AppEnv.Faults.Fatal("FX441",null,$"Invalid repository root path in the environment variable {EnvVarVigoRepositoryRoot}"));

            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(path);
            
            return new DirectoryInfo(path);
        }
        catch (Exception e) when (e is not VigoException)
        {
            throw new VigoFatalException(AppEnv.Faults.Fatal("FX448","Check what kind of exception was thrown and add proper handling for this situation", string.Empty), e);
        }
    }
    
    private static FileInfo? GetOutputFile(EnvVar envVarSource)
    {
        try
        {
            var path = envVarSource.GetEnvironmentVariable(EnvVarVigoOutputFile);

            Log.Debug("Environment variable {TheName} has value {TheValue}", EnvVarVigoOutputFile, path);

            if (path is null)
                return null;

            if (4096 < path.Length)
                path = path[..4096];

            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(path);
            
            var fileInfo = new FileInfo(path);
        
            if (fileInfo.Directory is null || !fileInfo.Directory.Exists) 
            {
                Log.Error("Expected the tarball file path to be located in an existing directory (Tarball file path: {ThePath})", path);
                throw new VigoFatalException(AppEnv.Faults.Fatal("FX455",null,$"The directory for the output file must exist. Check the environment variable {EnvVarVigoOutputFile}"));
            }

            if (!fileInfo.Name.EndsWith(".tar.gz") || fileInfo.Name.Length < 8) 
            {
                Log.Error("Expected the tarball file name to have a .tar.gz suffix and a non-empty base name (Tarball file name: {TheName})", fileInfo.Name);
                throw new VigoFatalException(AppEnv.Faults.Fatal("FX462",null, $"The output file name must have a .tar.gz suffix. Check the environment variable {EnvVarVigoOutputFile}"));
            }
            
            if (fileInfo.Exists)
                fileInfo.Delete();
        
            return fileInfo;
        }
        catch (Exception e) when (e is not VigoException)
        {
            throw new VigoFatalException(AppEnv.Faults.Fatal("FX469","Check what kind of exception was thrown and add proper handling for this situation", string.Empty), e);
        }
    }

    private static IReadOnlyList<string>? GetTargets(EnvVar envVarSource)
    {
        try
        {
            var targets = envVarSource.GetEnvironmentVariable(EnvVarVigoTargets);

            Log.Debug("Environment variable {TheName} has value {TheValue}", EnvVarVigoTargets, targets);

            if (targets is null)
                return null;

            if (4096 < targets.Length)
                targets = targets[..4096];

            var targetsList = targets.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(Path.GetFileName)
                .OfType<string>()
                .ToList();

            Log.Debug("Environment variable {TheName} was split into {TheSplitValues}", EnvVarVigoTargets, targetsList);
            
            if (0 == targetsList.Count)
                throw new VigoFatalException(AppEnv.Faults.Fatal("FX476",null,$"The list of targets cannot be empty, but you can delete the environment variable to handle all targets. Check the environment variable {EnvVarVigoTargets}"));

            var invalidTargets = targetsList.Where(t => !DeploymentTargetHelper.IsValidName(t)).ToList();
                
            if (0 < invalidTargets.Count)
            {
                Log.Error("{TheEnvVar} has invalid target names {TheInvalidNames}", EnvVarVigoTargets, invalidTargets);
                throw new VigoFatalException(AppEnv.Faults.Fatal("FX483",null, $"There are invalid target names {string.Join(", ", invalidTargets)} has invalid target names. Check the environment variable {EnvVarVigoTargets}"));
            }

            return targetsList;
        }
        catch (Exception e) when (e is not VigoException)
        {
            throw new VigoFatalException(AppEnv.Faults.Fatal("FX490","Check what kind of exception was thrown and add proper handling for this situation", string.Empty), e);
        }
    }
    
    private static FileInfo? GetLogfile(EnvVar envVarSource)
    {
        try
        {
            var path = envVarSource.GetEnvironmentVariable(EnvVarVigoLogfile);

            Log.Debug("Environment variable {TheName} has value {TheValue}", EnvVarVigoLogfile, path);

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
        
            Log.Warning("The logfile directory does not exist. Will skip logging. {TheLogFile}", fileInfo.FullName);
            return null;
        }
        catch (Exception e) when (e is not VigoException)
        {
            throw new VigoFatalException(AppEnv.Faults.Fatal("FX497","Check what kind of exception was thrown and add proper handling for this situation", string.Empty), e);
        }
    }

    private static LogEventLevel GetLogLevel(EnvVar envVarSource)
    {
        try
        {
            var value = envVarSource.GetEnvironmentVariable(EnvVarVigoLogLevel, "Information");

            Log.Debug("Environment variable {TheName} has value {TheValue}", EnvVarVigoLogLevel,value);

            if (string.IsNullOrWhiteSpace(value))
                return LogEventLevel.Information;
            
            if (40 < value.Length)
                value = value[..40];
        
            if (Enum.TryParse<LogEventLevel>(value, true, out var logLevel)) 
                return logLevel;
        
            logLevel = LogEventLevel.Information;

            Log.Warning("Warning: Invalid log level {TheGivenValue} specified. Will use {TheReplacement} instead", value, logLevel);

            return logLevel;
        }
        catch (Exception e) when (e is not VigoException)
        {
            throw new VigoFatalException(AppEnv.Faults.Fatal("FX504","Check what kind of exception was thrown and add proper handling for this situation", string.Empty), e);
        }
    }
    
    private static readonly char[] ListSeparators = new char[] { ' ', ',', ':' };
}