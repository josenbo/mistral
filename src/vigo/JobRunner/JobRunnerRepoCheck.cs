using Serilog;
using vigobase;
using vigorule;

namespace vigo;

internal class JobRunnerRepoCheck(AppConfigRepoCheck appConfigRepoCheck) : JobRunner
{
    public IRepositoryReader RepositoryReader => _reader;
    private AppConfigRepoCheck AppConfig { get; } = appConfigRepoCheck;

    public override bool Prepare()
    {
        _reader.Read();
        
        return _reader
            .FinalItems<IFinalHandling>(true)
            .All(ft => ft.CheckedSuccessfully);
    }

    public override bool Run()
    {
        Success = RunCommitChecks(_reader, AppConfig);
        return Success;
    }

    public override void CleanUp()
    {
        try
        {
            AppEnv.TemporaryDirectory.Delete(true);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Failed to clean up");
        }
    }

    private readonly IRepositoryReader _reader = RuleBasedHandlingApi.GetReader(
        topLevelDirectory: appConfigRepoCheck.RepositoryRoot,
        defaultHandling: AppEnv.DefaultFileHandlingParams,
        configFiles: AppEnv.DeployConfigRule.Filenames
        );

    private static bool RunCommitChecks(IRepositoryReader reader, AppConfigRepoCheck appConfig)
    {
        return true;
    }
}