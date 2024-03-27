using System.Reflection;

namespace vigo;

internal class JobRunnerVersion : JobRunner
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public JobRunnerVersion(AppConfigVersion appConfig)
    {
        AppConfig = appConfig;
    }

    protected override bool DoPrepare()
    {
        return true;
    }

    protected override bool DoRun()
    {
        Assembly appAssembly;
        
        switch (Assembly.GetEntryAssembly())
        {
            case { } notNullObject:
                appAssembly = notNullObject;
                break;
            default:
                return false;
        }

        var appVersion = appAssembly.GetName().Version;

        if (appVersion is null)
            return false;
        
        var currentExecutable = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        
        // Console.WriteLine($"{currentExecutable} v{appVersion.Major}.{appVersion.Minor}.{appVersion.Build}.{appVersion.Revision}");
        Console.WriteLine($"{currentExecutable} v{appVersion.Major}.{appVersion.Minor}.{appVersion.Build}");

        return true;
    }

    protected override void DoCleanUp()
    {
    }

    protected override string JobRunnerFailureMessage => "Failed to retrieve the version information";
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private AppConfigVersion AppConfig { get; }    
}