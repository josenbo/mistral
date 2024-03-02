using System.Diagnostics;
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
    var hasSingleRunLogConfiguration = ConfigSourceReaderEnvironmentVariables.TryGetSingleRunLogConfiguration(out var logLevelFile, out var logfile);

    if (!hasSingleRunLogConfiguration && !logLevelConsole.HasValue)
        return;
    
    var loggerConfiguration = new LoggerConfiguration()
        .MinimumLevel.Debug();

    if (logLevelConsole.HasValue)
    {
        loggerConfiguration.WriteTo.Console(restrictedToMinimumLevel: logLevelConsole.Value);
    }

    if (hasSingleRunLogConfiguration && logfile is not null)
    {
        // loggerConfiguration.WriteTo.File(logfile.FullName, restrictedToMinimumLevel: logLevelFile, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 4);
        loggerConfiguration.WriteTo.File(logfile.FullName, restrictedToMinimumLevel: logLevelFile);
    }

    Log.Logger = loggerConfiguration.CreateLogger();
}

