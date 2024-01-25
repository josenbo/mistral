using Serilog;
using Serilog.Events;
using vigo;

try
{
    var config = ConfigurationBuilder.ActiveConfiguration;

    ConfigureLogging(config.Logfile, config.LogLevel);

    var builder = new TarballBuilder(config);
    
    builder.Build();
    
    Environment.ExitCode = 0;
}
catch (Exception e)
{
    Log.Error(e, "program aborted");
    Console.Error.WriteLine("Fatal: the program terminates prematurely due to an error it could not handle");
    Environment.ExitCode = 1;
}

return;


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
