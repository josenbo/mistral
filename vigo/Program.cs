using System.Diagnostics.CodeAnalysis;
using Serilog;
using Serilog.Events;
using vigo;
using vigobase;

try
{
    AppEnv.TimingReportFile = GetFileInfoFromEnvironmentVariable("VIGO_TIMING_REPORT");
    
    AppEnv.CheckAndSetConfigurationFile(
        Environment.GetEnvironmentVariable("VIGO_FOLDERCONFIG_MARKDOWN"), 
        Environment.GetEnvironmentVariable("VIGO_FOLDERCONFIG_NATIVE"));
    
    AppEnv.RecordTiming("Application started");

    ConfigureLogging();

    var appConfig = AppConfigBuilder.Assemble(args);

    Log.Information("Running the action {TheCommand} with the configuration {TheConfig}",
        appConfig.Command,
        appConfig);

    JobRunner jobRunner = appConfig switch
    {
        AppConfigDeploy appConfigRepoDeploy => new JobRunnerDeploy(appConfigRepoDeploy),
        AppConfigExplain appConfigFolderExplain => new JobRunnerExplain(appConfigFolderExplain),
        AppConfigHelp appConfigInfoHelp => new JobRunnerHelp(appConfigInfoHelp),
        AppConfigVersion appConfigInfoVersion => new JobRunnerVersion(appConfigInfoVersion),
        null => throw new VigoFatalException(AppEnv.Faults.Fatal(
            "FX252", 
            $"Configuration should be valid or throw a fatal exception. Check why {nameof(appConfig)} could be null", 
            string.Empty)),
        _ => throw new VigoFatalException(AppEnv.Faults.Fatal(
            "FX259",
            "Are we missing a new app action that needs to be implemented?", 
            string.Empty))
    };

    jobRunner.Run();

    Log.Information("Process terminated {TheResult} after {TheTimeSpan}",
        (jobRunner.Success ? "successfully" : "with errors"),
        AppEnv.GetCurrentRunElapsedTime());

    Environment.ExitCode = jobRunner.Success ? 0 : 1;
}



catch (VigoFatalException e)
{
    Log.Fatal(e, "Immediate termination of the program was requested and we will exit after dumping the incidents");
    
    Console.Error.WriteLine("The program terminates prematurely due to the following incidents:");
    Console.Error.WriteLine();
    var printHeader = true;
    foreach (var incident in AppEnv.Faults.Incidents)
    {
        if (printHeader)
        {
            Console.Error.WriteLine("Log-Entry  Message");
            printHeader = false;
        }
            
        Console.Error.WriteLine($"{incident.IncidentId}  {incident.Message}");
    }
    Console.Error.WriteLine();
    Console.Error.WriteLine("Check (or enable) the logs and consult help pages");
    Console.Error.WriteLine();
    Environment.ExitCode = 1;
}



catch (Exception e)
{
    Log.Error(e, "program aborted");
    Console.Error.WriteLine("Fatal: the program terminates prematurely due to an error it could not handle");
    Environment.ExitCode = 1;
}



finally
{
    AppEnv.RecordTiming("Application terminates");
    Log.CloseAndFlush();
}

return;



////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

#region Setting up logging

void ConfigureLogging(LogEventLevel? logLevelConsole = null)
{
    if (TryGetLogLevelFromEnvironmentVariable("VIGO_CONSOLE_LOG_LEVEL", out var parsedConsoleLogLevel))
        logLevelConsole = parsedConsoleLogLevel.Value;
    
    TryGetLogfileFromEnvironmentVariable(
        environmentVariable: "VIGO_LOGFILE_DEBUG",
        deleteIfExists: true,
        logfile: out var logfileDebug);
    
    TryGetLogfileFromEnvironmentVariable(
        environmentVariable: "VIGO_LOGFILE_INFORMATION",
        deleteIfExists: true,
        logfile: out var logfileInformation);
    
    TryGetLogfileFromEnvironmentVariable(
        environmentVariable: "VIGO_LOGFILE_WARNING",
        deleteIfExists: true,
        logfile: out var logfileWarning);
    
    TryGetLogfileFromEnvironmentVariable(
        environmentVariable: "VIGO_LOGFILE_ERROR",
        deleteIfExists: true,
        logfile: out var logfileError);
    
    TryGetLogfileFromEnvironmentVariable(
        environmentVariable: "VIGO_SHARED_LOG_FILE",
        deleteIfExists: false,
        logfile: out var sharedLogFile);

    var sharedLogLevel = LogEventLevel.Information;
    var sharedLogRetentionDays = 5;
    
    if (sharedLogFile is not null)
    {
        if (TryGetLogLevelFromEnvironmentVariable("VIGO_SHARED_LOG_LEVEL", out var parsedLogLevel))
            sharedLogLevel = parsedLogLevel.Value;

        var parsedRetentionDays = EnvVar.GetSystem().GetEnvironmentVariable("VIGO_SHARED_LOG_RETENTION_DAYS");
        
        if (!string.IsNullOrWhiteSpace(parsedRetentionDays))
            if (int.TryParse(parsedRetentionDays, out var parsedInteger ))
                sharedLogRetentionDays = Math.Max(Math.Min(parsedInteger, 14), 1);
    }

    
    var minimumLevelSet = false;
    var loggerConfiguration = new LoggerConfiguration();

    // ----------------------------------------------------
    var configuredLogLevel = LogEventLevel.Verbose;
    
    if (logLevelConsole == configuredLogLevel || (sharedLogFile is not null && sharedLogLevel == configuredLogLevel))
    {
        loggerConfiguration.MinimumLevel.Verbose();
        minimumLevelSet = true;
        
        if (logLevelConsole == configuredLogLevel)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose);
    }

    // ----------------------------------------------------
    configuredLogLevel = LogEventLevel.Debug;
    
    if (logfileDebug is not null  || logLevelConsole == configuredLogLevel || (sharedLogFile is not null && sharedLogLevel == configuredLogLevel))
    {
        if (!minimumLevelSet)
        {
            loggerConfiguration.MinimumLevel.Debug();
            minimumLevelSet = true;
        }
        
        if (logLevelConsole == configuredLogLevel)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: configuredLogLevel);

        if (logfileDebug is not null)
            loggerConfiguration.WriteTo.File(logfileDebug.FullName, restrictedToMinimumLevel: configuredLogLevel);    
    }
        
    // ----------------------------------------------------
    configuredLogLevel = LogEventLevel.Information;

    if (logfileInformation is not null || logLevelConsole == configuredLogLevel || (sharedLogFile is not null && sharedLogLevel == configuredLogLevel))
    {
        if (!minimumLevelSet)
        {
            loggerConfiguration.MinimumLevel.Information();
            minimumLevelSet = true;
        }
        
        if (logLevelConsole == configuredLogLevel)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: configuredLogLevel);

        if (logfileDebug is not null)
            loggerConfiguration.WriteTo.File(logfileDebug.FullName, restrictedToMinimumLevel: configuredLogLevel);    
    }
        
    // ----------------------------------------------------
    configuredLogLevel = LogEventLevel.Warning;

    if (logfileWarning is not null || logLevelConsole == configuredLogLevel || (sharedLogFile is not null && sharedLogLevel == configuredLogLevel))
    {
        if (!minimumLevelSet)
        {
            loggerConfiguration.MinimumLevel.Warning();
            minimumLevelSet = true;
        }
        
        if (logLevelConsole == configuredLogLevel)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: configuredLogLevel);

        if (logfileDebug is not null)
            loggerConfiguration.WriteTo.File(logfileDebug.FullName, restrictedToMinimumLevel: configuredLogLevel);    
    }

    // ----------------------------------------------------
    configuredLogLevel = LogEventLevel.Error;

    if (logfileError is not null || logLevelConsole == configuredLogLevel || (sharedLogFile is not null && sharedLogLevel == configuredLogLevel))
    {
        if (!minimumLevelSet)
        {
            loggerConfiguration.MinimumLevel.Error();
            minimumLevelSet = true;
        }
        
        if (logLevelConsole == configuredLogLevel)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: configuredLogLevel);

        if (logfileDebug is not null)
            loggerConfiguration.WriteTo.File(logfileDebug.FullName, restrictedToMinimumLevel: configuredLogLevel);    
    }

    // ----------------------------------------------------
    configuredLogLevel = LogEventLevel.Fatal;

    if (logLevelConsole == configuredLogLevel || (sharedLogFile is not null && sharedLogLevel == configuredLogLevel))
    {
        if (!minimumLevelSet)
        {
            loggerConfiguration.MinimumLevel.Fatal();
            minimumLevelSet = true;
        }
        
        if (logLevelConsole == configuredLogLevel)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: configuredLogLevel);
    }

    if (!minimumLevelSet)
        return;
    
    if (sharedLogFile is not null)
    {
        var runId = Random.Shared.Next(1000000000, 1999999999);

        loggerConfiguration.WriteTo.File(
            sharedLogFile.FullName,
            restrictedToMinimumLevel: sharedLogLevel,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: sharedLogRetentionDays,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] #" + 
                            runId +
                            " {Message:lj}{NewLine}{Exception}");
    }
    
    Log.Logger = loggerConfiguration.CreateLogger();
}

#endregion

#region Environment variable helper functions

// ReSharper disable once UnusedLocalFunctionReturnValue
bool TryGetLogfileFromEnvironmentVariable(string environmentVariable, bool deleteIfExists, [NotNullWhen(true)]out FileInfo? logfile)
{
    var path = Environment.GetEnvironmentVariable(environmentVariable);
    
    if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path) || !Directory.Exists(Path.GetDirectoryName(path)))
    {
        logfile = null;
        return false;
    }

    path = Path.GetFullPath(path);
    
    if (deleteIfExists && File.Exists(path))
        File.Delete(path);
    
    logfile = new FileInfo(path);
    return true;
}

bool TryGetLogLevelFromEnvironmentVariable(string environmentVariable, [NotNullWhen(true)] out LogEventLevel? loglevel)
{
    var value = Environment.GetEnvironmentVariable(environmentVariable);

    if (string.IsNullOrWhiteSpace(value) || !Enum.TryParse<LogEventLevel>(value, true, out var parsedLoglevel))
    {
        loglevel = null;
        return false;
    }

    loglevel = parsedLoglevel;
    return true;
}

FileInfo? GetFileInfoFromEnvironmentVariable(string environmentVariable)
{
    var path = Environment.GetEnvironmentVariable(environmentVariable);
    
    if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path) || !Directory.Exists(Path.GetDirectoryName(path)))
        return null;

    return new FileInfo(Path.GetFullPath(path));
}

#endregion
