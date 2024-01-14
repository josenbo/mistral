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
        var nameParser = ConfigureNameParser(new NameParser(), config); 
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

INameParser ConfigureNameParser(NameParser nameParser, IVigoConfig config)
{
    nameParser.AddCaseSensitiveTags(config.NameParserConfig.CaseSensitiveFilterTags);
    nameParser.AddCaseInsensitiveTags(config.NameParserConfig.CaseInsensitiveFilterTags);
    return nameParser;
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
