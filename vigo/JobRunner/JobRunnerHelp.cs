using System.Reflection;
using System.Text;
using Serilog;

namespace vigo;

internal class JobRunnerHelp : JobRunner
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public JobRunnerHelp(AppConfigHelp appConfig)
    {
        AppConfig = appConfig;
    }
    
    protected override bool DoPrepare()
    {
        const string name = "StandardHelpScreen.txt";

        Log.Debug("Looking for the general help screen in the embedded resource file {TheResource}",
            name);

        var resourceName = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceNames()
            .SingleOrDefault(n => n.EndsWith(name));            
            
        _helpResource = resourceName ?? string.Empty;
        return (resourceName is not null);
    }

    protected override bool DoRun()
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

    protected override void DoCleanUp()
    {
    }
    
    protected override string JobRunnerFailureMessage => "Failed to retrieve the help page";
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private AppConfigHelp AppConfig { get; }
    private string _helpResource = string.Empty;
}