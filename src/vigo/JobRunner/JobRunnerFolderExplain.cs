namespace vigo;

internal class JobRunnerFolderExplain(AppConfigFolderExplain appConfig) : JobRunner
{
    private AppConfigFolderExplain AppConfig { get; } = appConfig;    

    public override bool Prepare() => true;

    public override bool Run() => true;

    public override void CleanUp()
    {
    }
}