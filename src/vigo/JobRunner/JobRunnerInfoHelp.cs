using System.Reflection;
using System.Text;
using Serilog;
using Serilog.Events;
using vigobase;

namespace vigo;

internal class JobRunnerInfoHelp(AppConfigInfoHelp appConfig) : IJobRunner
{
    public bool Success { get; } = true;

    public bool Prepare()
    {
        const string defaultPage = "HelpGeneral.txt";
        
        var name = AppConfig.CommandToShowHelpFor switch
        {
            CommandEnum.Deploy => "HelpDeploy.txt",
            CommandEnum.Check => "HelpCheck.txt",
            CommandEnum.Explain => "HelpExplain.txt",
            _ => defaultPage
        };

        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Debug("Looking for help on {TheTopic} in the embedded resource file {TheResource}",
                AppConfig.CommandToShowHelpFor,
                name);

            foreach (var resname in Assembly
                         .GetExecutingAssembly()
                         .GetManifestResourceNames())
            {
                var marker = resname.EndsWith(name) ? "FOUND" : "     ";
                Log.Debug("Check embedded resource: {TheMarker} {TheResource}",
                    marker,
                    resname);
            }
        }
        
        var resourceName = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceNames()
            .SingleOrDefault(n => n.EndsWith(name));

        if (resourceName is null && name != defaultPage)
        {
            resourceName = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceNames()
                .SingleOrDefault(n => n.EndsWith(defaultPage));            
        }

        _helpResource = resourceName ?? string.Empty;
        return (resourceName is not null);
    }

    public bool Run()
    {
        if (string.IsNullOrWhiteSpace(_helpResource))
            return false;

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_helpResource);
        if (stream is null)
            return false;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var helpText = reader.ReadToEnd();
        Console.Write(helpText);

        return true;
    }

    public void CleanUp()
    {
    }
    
    private AppConfigInfoHelp AppConfig { get; } = appConfig;
    private string _helpResource = string.Empty;
}