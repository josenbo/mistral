using Serilog;
using vigobase;
using vigorule;

namespace vigo;

internal class JobRunnerDeploy : JobRunner
{
    public JobRunnerDeploy(AppConfigDeploy appConfigDeploy)
    {
        AppConfig = appConfigDeploy;
        AppEnv.TopLevelDirectory = AppConfig.RepositoryRoot;
        Success = false;
        _reader = RuleBasedHandlingApi.GetReader(
            topLevelDirectory: appConfigDeploy.RepositoryRoot,
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
        Success = BuildTarball(_reader, AppEnv.TemporaryDeploymentBundle, AppConfig.Targets, false);

        return Success;
    }

    public override void CleanUp()
    {
        try
        {
            if (AppConfig.DeploymentBundle is not null)
            {
                AppEnv.TemporaryDeploymentBundle.Refresh();
            
                if (Success && AppEnv.TemporaryDeploymentBundle.Exists)
                {
                    Log.Debug("Moving the archive file from the temporary folder to the target destination {TheTarget}", 
                        AppConfig.DeploymentBundle);
                    File.Move(AppEnv.TemporaryDeploymentBundle.FullName, AppConfig.DeploymentBundle.FullName);
                }
            }
            
            AppEnv.TemporaryDirectory.Delete(true);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Failed to clean up");
        }
    }

    private AppConfigDeploy AppConfig { get; }

    private readonly IRepositoryReader _reader;

}