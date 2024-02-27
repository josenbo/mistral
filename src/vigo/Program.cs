using System.Diagnostics;
using Serilog;
using Serilog.Events;
using vigo;

try
{
    var stopwatch = new Stopwatch();
    stopwatch.Start();

    ConfigureLogging();

    var settings = AppSettingsBuilder.AppConfigRepo;
    
    Log.Information("Running the command {TheCommand} with the repository root folder {TheRepositoryRoot}",
        settings.Command,
        settings.RepositoryRoot.FullName);

    var jobRunner = new JobRunnerDoNothing(settings);
    
    if (jobRunner.Prepare())
        jobRunner.Run();

    jobRunner.CleanUp();
    
    stopwatch.Stop();
    Log.Information("Process terminated {TheResult} after {TheTimeSpan}",
        (jobRunner.Success ? "successfully" : "with errors"),
        stopwatch.Elapsed);

    Environment.ExitCode = jobRunner.Success ? 0 : 1;
}
catch (Exception e)
{
    Log.Error(e, "program aborted");
    Console.Error.WriteLine("Fatal: the program terminates prematurely due to an error it could not handle");
    Environment.ExitCode = 1;
}

Log.CloseAndFlush();
return;

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

void ConfigureLogging(LogEventLevel? logLevelConsole = null)
{
    var hasSingleRunLogConfiguration = AppSettingsBuilder.TryGetSingleRunLogConfiguration(out var logLevelFile, out var logfile);

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
