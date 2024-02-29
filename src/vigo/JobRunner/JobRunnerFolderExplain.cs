namespace vigo;

internal class JobRunnerFolderExplain(AppConfigFolderExplain appConfig) : IJobRunner
{
    private AppConfigFolderExplain AppConfig { get; } = appConfig;    

    public bool Success { get; } = true;
    
    public bool Prepare() => true;

    public bool Run() => true;

    public void CleanUp()
    {
    }
}