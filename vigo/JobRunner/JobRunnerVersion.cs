using System.Diagnostics;
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

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(appAssembly.Location);
        var productVersion = fileVersionInfo.ProductVersion;

        if (productVersion is null)
            return false;
        
        var currentExecutable = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        Console.WriteLine($"{currentExecutable} {productVersion}");
        
        return true;
    }

    protected override void DoCleanUp()
    {
    }

    protected override string JobRunnerFailureMessage => "Failed to retrieve the version information";
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private AppConfigVersion AppConfig { get; }    
}