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
        _reader = RuleBasedHandlingApi.GetReader(
            topLevelDirectory: appConfigDeploy.RepositoryRoot,
            includePreview: appConfigDeploy.Preview,
            onlyTopLeveDirectory: false,
            defaultHandling: AppEnv.DefaultFileHandlingParams,
            configFiles: AppEnv.DeployConfigRule.Filenames);
    }

    protected override bool DoPrepare()
    {
        _reader.Read();

        return _reader
            .FinalItems<IFinalHandling>(true)
            .All(ft => ft.CheckedSuccessfully);
    }
    
    protected override bool DoRun()
    {
        if (!BuildTarball(_reader, AppEnv.TemporaryDeploymentBundle, AppConfig.Targets, false))
            return false;

        if (AppConfig.DeploymentBundle is null) 
            return true;
        
        AppEnv.TemporaryDeploymentBundle.Refresh();

        if (!AppEnv.TemporaryDeploymentBundle.Exists) 
            return true;
        
        Log.Debug("Moving the archive file from the temporary folder to the target destination {TheTarget}", 
            AppConfig.DeploymentBundle);
        
        File.Move(AppEnv.TemporaryDeploymentBundle.FullName, AppConfig.DeploymentBundle.FullName);

        return true;
    }

    protected override void DoCleanUp()
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

    protected override string JobRunnerFailureMessage => "Failed to build the deployment bundle";
    private AppConfigDeploy AppConfig { get; }

    private readonly IRepositoryReader _reader;
}