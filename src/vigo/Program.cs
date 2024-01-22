
Console.WriteLine("vigo console application needs to be re-enabled after the refactorings");

/*
using Serilog;
using Serilog.Events;
using vigocfg;
using vigoftg;
using vigolib;

var ok = ConfigureLogging(LogEventLevel.Information);

if (ok)
{
    try
    {
        var config = (IVigoConfig)new VigoConfig(null);
        var nameParser = ConfigureNameParser(new NameParserFactory(), config); 
        var job = (IVigoJob)new VigoJob(config, nameParser);

        var jobResult = job.Run();
        
        ok = jobResult.Success;
    }
    catch (Exception e)
    {
        Log.Error(e, "program aborted");
        ok = false;
    }
}

Environment.ExitCode = ok ? 0 : 1;

return;

INameParser ConfigureNameParser(NameParserFactory factory, IVigoConfig config)
{
    factory.AddTags(config.NameParserConfig.CaseSensitiveFilterTags);
    factory.AddTags(config.NameParserConfig.CaseInsensitiveFilterTags);
    return factory.Build();
}

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

*/