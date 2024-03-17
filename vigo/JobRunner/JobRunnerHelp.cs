using System.Reflection;
using System.Text;
using Serilog;
using Serilog.Events;
using vigobase;

namespace vigo;

internal class JobRunnerHelp : JobRunner
{
    public JobRunnerHelp(AppConfigHelp appConfig)
    {
        AppConfig = appConfig;
        Success = false;
    }
    
    public override bool Prepare()
    {
        const string name = "StandardHelpScreen.txt";

        Log.Debug("Looking for the general help screen in the embedded resource file {TheResource}",
            name);

        var resourceName = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceNames()
            .SingleOrDefault(n => n.EndsWith(name));            
            
        _helpResource = resourceName ?? string.Empty;
        Success = (resourceName is not null);
        return Success;
    }

    public override bool Run()
    {
        if (string.IsNullOrWhiteSpace(_helpResource))
            return Success = false;

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_helpResource);
        if (stream is null)
            return false;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var helpText = reader.ReadToEnd();
        Console.Write(helpText);

        return Success = true;
    }

    public override void CleanUp()
    {
    }
    
    private AppConfigHelp AppConfig { get; }
    private string _helpResource = string.Empty;
}