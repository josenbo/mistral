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
        appConfig.ToString());

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
    if (TryGetLogLevelFromEnvironmentVariable("VIGO_CONSOLE_LOGLEVEL", out var parsedConsoleLogLevel))
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
        environmentVariable: "VIGO_SHARED_LOGFILE",
        deleteIfExists: false,
        logfile: out var sharedLogFile);

    var sharedLogLevel = LogEventLevel.Information;
    var sharedLogRetentionDays = 5;
    
    if (sharedLogFile is not null)
    {
        if (TryGetLogLevelFromEnvironmentVariable("VIGO_SHARED_LOGLEVEL", out var parsedLogLevel))
            sharedLogLevel = parsedLogLevel.Value;

        var parsedRetentionDays = EnvVar.GetSystem().GetEnvironmentVariable("VIGO_SHARED_RETENTION_DAYS");
        
        if (!string.IsNullOrWhiteSpace(parsedRetentionDays))
            if (int.TryParse(parsedRetentionDays, out var parsedInteger ))
                sharedLogRetentionDays = Math.Max(Math.Min(parsedInteger, 14), 1);
    }

    var debugLoggingEnabled = false;
    var informationLoggingEnabled = false;
    var warningLoggingEnabled = false;
    var errorLoggingEnabled = false;
    var sharedLoggingEnabled = false;
    var sharedRunId = Random.Shared.Next(1000000000, 1999999999);
    
    LogEventLevel? minimumLevel = null;
    var loggerConfiguration = new LoggerConfiguration();

    // ----------------------------------------------------
    var configuredLogLevel = LogEventLevel.Verbose;
    
    if (logLevelConsole == configuredLogLevel || (sharedLogFile is not null && sharedLogLevel == configuredLogLevel))
    {
        loggerConfiguration.MinimumLevel.Verbose();
        minimumLevel= configuredLogLevel;
        
        if (logLevelConsole == configuredLogLevel)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose);
    }

    // ----------------------------------------------------
    configuredLogLevel = LogEventLevel.Debug;
    
    if (logfileDebug is not null  || logLevelConsole == configuredLogLevel || (sharedLogFile is not null && sharedLogLevel == configuredLogLevel))
    {
        if (minimumLevel is null)
        {
            loggerConfiguration.MinimumLevel.Debug();
            minimumLevel= configuredLogLevel;
        }
        
        if (logLevelConsole == configuredLogLevel)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: configuredLogLevel);

        if (logfileDebug is not null)
        {
            loggerConfiguration.WriteTo.File(logfileDebug.FullName, restrictedToMinimumLevel: configuredLogLevel);
            debugLoggingEnabled = true;
        }    
    }
        
    // ----------------------------------------------------
    configuredLogLevel = LogEventLevel.Information;

    if (logfileInformation is not null || logLevelConsole == configuredLogLevel || (sharedLogFile is not null && sharedLogLevel == configuredLogLevel))
    {
        if (minimumLevel is null)
        {
            loggerConfiguration.MinimumLevel.Information();
            minimumLevel= configuredLogLevel;
        }
        
        if (logLevelConsole == configuredLogLevel)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: configuredLogLevel);

        if (logfileInformation is not null)
        {
            loggerConfiguration.WriteTo.File(logfileInformation.FullName, restrictedToMinimumLevel: configuredLogLevel);
            informationLoggingEnabled = true;
        }    
    }
        
    // ----------------------------------------------------
    configuredLogLevel = LogEventLevel.Warning;

    if (logfileWarning is not null || logLevelConsole == configuredLogLevel || (sharedLogFile is not null && sharedLogLevel == configuredLogLevel))
    {
        if (minimumLevel is null)
        {
            loggerConfiguration.MinimumLevel.Warning();
            minimumLevel= configuredLogLevel;
        }
        
        if (logLevelConsole == configuredLogLevel)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: configuredLogLevel);

        if (logfileWarning is not null)
        {
            loggerConfiguration.WriteTo.File(logfileWarning.FullName, restrictedToMinimumLevel: configuredLogLevel);
            warningLoggingEnabled = true;
        }    
    }

    // ----------------------------------------------------
    configuredLogLevel = LogEventLevel.Error;

    if (logfileError is not null || logLevelConsole == configuredLogLevel || (sharedLogFile is not null && sharedLogLevel == configuredLogLevel))
    {
        if (minimumLevel is null)
        {
            loggerConfiguration.MinimumLevel.Error();
            minimumLevel= configuredLogLevel;
        }
        
        if (logLevelConsole == configuredLogLevel)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: configuredLogLevel);

        if (logfileError is not null)
        {
            loggerConfiguration.WriteTo.File(logfileError.FullName, restrictedToMinimumLevel: configuredLogLevel);
            errorLoggingEnabled = true;
        }    
    }

    // ----------------------------------------------------
    configuredLogLevel = LogEventLevel.Fatal;

    if (logLevelConsole == configuredLogLevel || (sharedLogFile is not null && sharedLogLevel == configuredLogLevel))
    {
        if (minimumLevel is null)
        {
            loggerConfiguration.MinimumLevel.Fatal();
            minimumLevel= configuredLogLevel;
        }
        
        if (logLevelConsole == configuredLogLevel)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: configuredLogLevel);
    }

    if (minimumLevel is null)
        return;
    
    if (sharedLogFile is not null)
    {
        loggerConfiguration.WriteTo.File(
            sharedLogFile.FullName,
            restrictedToMinimumLevel: sharedLogLevel,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: sharedLogRetentionDays,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] #" + 
                            sharedRunId +
                            " {Message:lj}{NewLine}{Exception}");

        sharedLoggingEnabled = true;
    }
    
    Log.Logger = loggerConfiguration.CreateLogger();
    
    Log.Information("Logging is configured with a minimum log level of {TheMinimumLogLevel}", minimumLevel);
    
    if (logLevelConsole is null)
        Log.Information("Console logging is OFF");
    else
        Log.Information("Console logging is ON with as log level of {TheLogLevel}", logLevelConsole);
    
    
    Log.Information("Debug logging is {TheState} writing to {TheLogFile}", 
        (debugLoggingEnabled ? "ON" : "OFF"),
        logfileDebug);
    
    Log.Information("Information logging is {TheState} writing to {TheLogFile}", 
        (informationLoggingEnabled ? "ON" : "OFF"),
        logfileInformation);
    
    Log.Information("Warning logging is {TheState} writing to {TheLogFile}", 
        (warningLoggingEnabled ? "ON" : "OFF"),
        logfileWarning);
    
    Log.Information("Error logging is {TheState} writing to {TheLogFile}", 
        (errorLoggingEnabled ? "ON" : "OFF"),
        logfileError);

    if (sharedLoggingEnabled)
    {
        Log.Information(
            "Shared logging is ON with log level {TheLogLevel}, {TheRetention} days retention policy and run id #{TheRunId} writing to {TheLogFile}",
            sharedLogLevel,
            sharedLogRetentionDays,
            sharedRunId,
            sharedLogFile);
    }
    else Log.Information("Shared logging is OFF");
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
