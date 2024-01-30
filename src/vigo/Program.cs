using Serilog;
using Serilog.Events;
using vigo;

try
{
    var config = ConfigurationBuilder.ActiveConfiguration;

    ConfigureLogging(config.Logfile, config.LogLevel);

    var result = config switch
    {
        ConfigurationDeployToTarball configurationDeployToTarball => BuildTarball(configurationDeployToTarball),
        ConfigurationCheckCommit configurationCheckCommit => RunCommitChecks(configurationCheckCommit),
        _ => false
    };
    
    Environment.ExitCode = result ? 0 : 1;
}
catch (Exception e)
{
    Log.Error(e, "program aborted");
    Console.Error.WriteLine("Fatal: the program terminates prematurely due to an error it could not handle");
    Environment.ExitCode = 1;
}

return;

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
        loggerConfiguration.WriteTo.File(logfile.FullName, restrictedToMinimumLevel: logLevelFile, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 4);
    }

    Log.Logger = loggerConfiguration.CreateLogger();
}
