using System.Diagnostics;
using Serilog;
using Serilog.Events;
using vigo;

try
{
    var stopwatch = new Stopwatch();
    stopwatch.Start();

    var config = ConfigurationBuilder.ActiveConfiguration;

    Environment.ExitCode = 1;

    if (config.Logfile is not null && File.Exists(config.Logfile.FullName))
        config.Logfile.Delete();

    ConfigureLogging(config.Logfile, config.LogLevel);
    
    Log.Information("Running the command {TheCommand} with the repository root folder {TheRepositoryRoot}",
        config.Command,
        config.RepositoryRoot.FullName);

    try
    {
        var result = config switch
        {
            ConfigurationDeployToTarball configurationDeployToTarball => BuildTarball(configurationDeployToTarball),
            ConfigurationCheckCommit configurationCheckCommit => RunCommitChecks(configurationCheckCommit),
            _ => false
        };

        Log.Information("Process terminated {TheResult}",
            (result ? "successfully" : "with errors"));

        stopwatch.Stop();
        Log.Information("Process duration was {TheTimeSpan}",
            stopwatch.Elapsed);

        Environment.ExitCode = result ? 0 : 1;
    }
    finally
    {
        try
        {
            if (config is ConfigurationDeployToTarball configTarball && File.Exists(configTarball.TemporaryTarballPath))
                File.Move(configTarball.TemporaryTarballPath, configTarball.Tarball.FullName);
            
            config.TemporaryDirectory.Delete(true);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Could not delete the temporary folder ({e.GetType().Name}: {e.Message})");
        }
    }
    stopwatch.Stop();
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

bool RunCommitChecks(ConfigurationCheckCommit config)
{
    return true;
}

bool BuildTarball(ConfigurationDeployToTarball config)
{
    try
    {
        var builder = new TarballBuilder(config);
    
        builder.Build();

        return true;
    }
    catch (Exception e)
    {
        Log.Fatal(e, "Failed to deploy repository files to a tarball. The tarball file might be incomplete and will be deleted, if it exists");
        Console.Error.WriteLine("Fatal: the program terminates prematurely due to an unhandled error");

        try
        {
            config.Tarball.Refresh();
            if (config.Tarball.Exists)
                config.Tarball.Delete();
        }
        catch (Exception)
        {
            // ignore errors during cleanup
        }

        return false;
    }
}

void ConfigureLogging(FileInfo? logfile, LogEventLevel logLevelFile, LogEventLevel logLevelConsole = LogEventLevel.Warning)
{
    var loggerConfiguration = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console(restrictedToMinimumLevel: logLevelConsole);

    if (logfile is not null)
    {
        // loggerConfiguration.WriteTo.File(logfile.FullName, restrictedToMinimumLevel: logLevelFile, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 4);
        loggerConfiguration.WriteTo.File(logfile.FullName, restrictedToMinimumLevel: logLevelFile);
    }

    Log.Logger = loggerConfiguration.CreateLogger();
}
