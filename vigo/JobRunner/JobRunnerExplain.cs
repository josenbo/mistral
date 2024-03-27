using vigobase;

namespace vigo;

internal class JobRunnerExplain : JobRunner
{
    public JobRunnerExplain(AppConfigExplain appConfig)
    {
        AppConfig = appConfig;
        AppEnv.TopLevelDirectory = AppConfig.RepositoryRoot;
    }
    
    private AppConfigExplain AppConfig { get; }    

    protected override bool DoPrepare()
    {
        return true;
    }

    protected override bool DoRun()
    {
        return true;
    }

    protected override void DoCleanUp()
    {
    }

    protected override string JobRunnerFailureMessage => "Failed to explain the file handling";
}