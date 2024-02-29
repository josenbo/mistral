using System.Reflection;

namespace vigo;

internal class JobRunnerInfoVersion(AppConfigInfoVersion appConfig) : IJobRunner
{
    public bool Success { get; } = true;
    
    public bool Prepare() => true;

    public bool Run()
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

    public void CleanUp()
    {
    }

    // ReSharper disable once UnusedMember.Local
    private AppConfigInfoVersion AppConfig { get; } = appConfig;    
}