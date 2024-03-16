using System.Reflection;

namespace vigo;

internal class JobRunnerInfoVersion(AppConfigInfoVersion appConfig) : JobRunner
{
    public override bool Prepare() => true;

    public override bool Run()
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

    public override void CleanUp()
    {
    }

    // ReSharper disable once UnusedMember.Local
    private AppConfigInfoVersion AppConfig { get; } = appConfig;    
}