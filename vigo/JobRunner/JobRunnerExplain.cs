using vigobase;

namespace vigo;

internal class JobRunnerExplain : JobRunner
{
    public JobRunnerExplain(AppConfigExplain appConfig)
    {
        AppConfig = appConfig;
        AppEnv.TopLevelDirectory = AppConfig.RepositoryRoot;
        Success = false;
    }
    
    private AppConfigExplain AppConfig { get; }    

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