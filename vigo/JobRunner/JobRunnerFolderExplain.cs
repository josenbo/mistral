namespace vigo;

internal class JobRunnerFolderExplain(AppConfigFolderExplain appConfig) : JobRunner
{
    private AppConfigFolderExplain AppConfig { get; } = appConfig;    

    public override bool Prepare()
    {
        return Success = true;
    }

    public override bool Run()
    {
        return Success = true;
    }

    public override void CleanUp()
    {
    }
}