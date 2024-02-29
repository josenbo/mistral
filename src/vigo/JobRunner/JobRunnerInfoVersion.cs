namespace vigo;

internal class JobRunnerInfoVersion(AppConfigInfoVersion appConfig) : IJobRunner
{
    private AppConfigInfoVersion AppConfig { get; } = appConfig;    

    public bool Success { get; } = true;
    
    public bool Prepare() => true;

    public bool Run() => true;

    public void CleanUp()
    {
    }
}