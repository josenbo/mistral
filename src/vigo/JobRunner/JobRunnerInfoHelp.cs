namespace vigo;

internal class JobRunnerInfoHelp(AppConfigInfoHelp appConfig) : IJobRunner
{
    private AppConfigInfoHelp AppConfig { get; } = appConfig;    

    public bool Success { get; } = true;
    
    public bool Prepare() => true;

    public bool Run() => true;

    public void CleanUp()
    {
    }
}