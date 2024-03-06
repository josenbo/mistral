using Serilog;
using vigobase;
using vigorule;

namespace vigo;

internal class JobRunnerRepoCheck : JobRunner
{
    public JobRunnerRepoCheck(AppConfigRepoCheck appConfigRepoCheck)
    {
        AppConfig = appConfigRepoCheck;
        AppEnv.TopLevelDirectory = AppConfig.RepositoryRoot;
        Success = false;
        _reader = RuleBasedHandlingApi.GetReader(
            topLevelDirectory: appConfigRepoCheck.RepositoryRoot,
            defaultHandling: AppEnv.DefaultFileHandlingParams,
            configFiles: AppEnv.DeployConfigRule.Filenames);
    }
    
    public override bool Prepare()
    {
        _reader.Read();

        Success = _reader
            .FinalItems<IFinalHandling>(true)
            .All(ft => ft.CheckedSuccessfully);

        return Success;
    }
    
    public override bool Run()
    {
        // the check job will always build all targets. So the targets filter is always empty
        Success = BuildTarball(_reader, new FileInfo(AppConfigRepo.OutputFileTempPath), Array.Empty<string>(), true);

        return Success;
    }

    public override void CleanUp()
    {
        try
        {
            // in contrast to the deploy job, we just leave the archive in the temp folder
            // (which will be deleted in the next step)
            
            AppEnv.TemporaryDirectory.Delete(true);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Failed to clean up");
        }
    }

    private AppConfigRepoCheck AppConfig { get; }

    private readonly IRepositoryReader _reader;
}