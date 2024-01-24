using Serilog;
using Serilog.Events;
using vigo;

// Console.WriteLine("vigo console application needs to be re-enabled after the refactorings");

var ok = ConfigureLogging(LogEventLevel.Information);

if (ok)
{
    try
    {
        var job = (IJob)new ArchiveJob();
        Log.Information("Running the job {JobClass}", job);
        
        ok = job.Run();
    }
    catch (Exception e)
    {
        Log.Error(e, "program aborted");
        ok = false;
    }
}

Environment.ExitCode = ok ? 0 : 1;

return;


bool ConfigureLogging(LogEventLevel? consoleLogEventLevel)
{
    try
    {
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(restrictedToMinimumLevel: consoleLogEventLevel ?? LogEventLevel.Information);

        var envLogFolder = Environment.GetEnvironmentVariable("VIGO_LOG_FOLDER");

        if (!string.IsNullOrWhiteSpace(envLogFolder) && Directory.Exists(envLogFolder))
        {
            var logFile = new FileInfo(Path.Combine(envLogFolder, "vigo.log"));
            loggerConfiguration.WriteTo.File(logFile.FullName, restrictedToMinimumLevel: LogEventLevel.Debug, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 2);
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        return true;
    }
    catch (Exception e)
    {
        Console.WriteLine($"Could not configure logging: {e.Message}");
        return false;
    }
}
