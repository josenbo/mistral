using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Serilog;
using Serilog.Events;
using vigo;
using vigobase;

var stopwatch = new Stopwatch();
stopwatch.Start();

try
{
    ConfigureLogging();

    var settings = AppConfigBuilder.Build();

    Log.Information("Running the action {TheCommand} with the configuration {TheConfig}",
        settings.Command,
        settings);

    JobRunner jobRunner = settings switch
    {
        AppConfigRepoDeploy appConfigRepoDeploy => new JobRunnerRepoDeploy(appConfigRepoDeploy),
        AppConfigRepoCheck appConfigRepoCheck => new JobRunnerRepoCheck(appConfigRepoCheck),
        AppConfigFolderExplain appConfigFolderExplain => new JobRunnerFolderExplain(appConfigFolderExplain),
        AppConfigInfoHelp appConfigInfoHelp => new JobRunnerInfoHelp(appConfigInfoHelp),
        AppConfigInfoVersion appConfigInfoVersion => new JobRunnerInfoVersion(appConfigInfoVersion),
        null => throw new VigoFatalException(AppEnv.Faults.Fatal(
            "FX252", 
            $"Configuration should be valid or throw a fatal exception. Check why {nameof(settings)} could be null", 
            string.Empty)),
        _ => throw new VigoFatalException(AppEnv.Faults.Fatal(
            "FX259",
            "Are we missing a new app action that needs to be implemented?", 
            string.Empty))
    };

    try
    {
        if (jobRunner.Prepare())
            jobRunner.Run();
    }
    finally
    {
        jobRunner.CleanUp();
    }

    Log.Information("Process terminated {TheResult} after {TheTimeSpan}",
        (jobRunner.Success ? "successfully" : "with errors"),
        stopwatch.Elapsed);

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
    stopwatch.Stop();
    Log.CloseAndFlush();
}

return;

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

void ConfigureLogging(LogEventLevel? logLevelConsole = null)
{
    TryGetLogfileFromEnvironmentVariable("VIGO_LOGFILE_DEBUG", out var logfileDebug);
    TryGetLogfileFromEnvironmentVariable("VIGO_LOGFILE_INFORMATION", out var logfileInformation);
    TryGetLogfileFromEnvironmentVariable("VIGO_LOGFILE_WARNING", out var logfileWarning);
    TryGetLogfileFromEnvironmentVariable("VIGO_LOGFILE_ERROR", out var logfileError);
    TryGetLogfileFromEnvironmentVariable("VIGO_LOGFILE_DEBUG_SHARED", out var logfileDebugShared);

    var minimumLevelSet = false;
    var loggerConfiguration = new LoggerConfiguration();

    if (logLevelConsole == LogEventLevel.Verbose)
    {
        loggerConfiguration.MinimumLevel.Verbose();
        minimumLevelSet = true;
        
        loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose);
    }
    
    if (logfileDebug is not null || logfileDebugShared is not null || logLevelConsole == LogEventLevel.Debug)
    {
        if (!minimumLevelSet)
        {
            loggerConfiguration.MinimumLevel.Debug();
            minimumLevelSet = true;
        }
        
        if (logLevelConsole == LogEventLevel.Debug)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug);

        if (logfileDebug is not null)
            loggerConfiguration.WriteTo.File(logfileDebug.FullName, restrictedToMinimumLevel: LogEventLevel.Debug);    
        
        // pattern for rolling logs for later use
        // loggerConfiguration.WriteTo.File(logfile.FullName, restrictedToMinimumLevel: logLevelFile, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 4);
        if (logfileDebugShared is not null)
        {
            var runId = Random.Shared.Next(1000000000, 1999999999);

            loggerConfiguration.WriteTo.File(
                logfileDebugShared.FullName,
                restrictedToMinimumLevel: LogEventLevel.Debug,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 4,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " + 
                                runId +
                                " {Message:lj}{NewLine}{Exception}");
        }
    }
        
    if (logfileInformation is not null || logLevelConsole == LogEventLevel.Information)
    {
        if (!minimumLevelSet)
        {
            loggerConfiguration.MinimumLevel.Information();
            minimumLevelSet = true;
        }
        
        if (logLevelConsole == LogEventLevel.Information)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information);

        if (logfileInformation is not null)
            loggerConfiguration.WriteTo.File(logfileInformation.FullName, restrictedToMinimumLevel: LogEventLevel.Information);    
    }
        
    if (logfileWarning is not null || logLevelConsole == LogEventLevel.Warning)
    {
        if (!minimumLevelSet)
        {
            loggerConfiguration.MinimumLevel.Warning();
            minimumLevelSet = true;
        }
        
        if (logLevelConsole == LogEventLevel.Warning)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Warning);

        if (logfileWarning is not null)
            loggerConfiguration.WriteTo.File(logfileWarning.FullName, restrictedToMinimumLevel: LogEventLevel.Warning);    
    }

    if (logfileError is not null || logLevelConsole == LogEventLevel.Error)
    {
        if (!minimumLevelSet)
        {
            loggerConfiguration.MinimumLevel.Error();
            minimumLevelSet = true;
        }
        
        if (logLevelConsole == LogEventLevel.Error)
            loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Error);

        if (logfileError is not null)
            loggerConfiguration.WriteTo.File(logfileError.FullName, restrictedToMinimumLevel: LogEventLevel.Error);    
    }

    if (logLevelConsole == LogEventLevel.Fatal)
    {
        if (!minimumLevelSet)
        {
            loggerConfiguration.MinimumLevel.Fatal();
            minimumLevelSet = true;
        }
        
        loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Fatal);
    }

    if (!minimumLevelSet)
        return;
    
    // pattern for rolling logs for later use
    // loggerConfiguration.WriteTo.File(logfile.FullName, restrictedToMinimumLevel: logLevelFile, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 4);

    if (logfileDebugShared is null)
    {
        Log.Logger = loggerConfiguration.CreateLogger();
        return;
    }
    
    var sharedLogger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .Enrich.
}

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

bool TryGetConsoleLogLevelFromEnvironmentVariable(string environmentVariable, [NotNullWhen(true)] out LogEventLevel? loglevel)
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

