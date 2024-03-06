using Serilog;
using vigobase;
using vigorule;

namespace vigo;

internal class JobRunnerRepoDeploy : JobRunner
{
    public JobRunnerRepoDeploy(AppConfigRepoDeploy appConfigRepoDeploy)
    {
        AppConfig = appConfigRepoDeploy;
        AppEnv.TopLevelDirectory = AppConfig.RepositoryRoot;
        Success = false;
        _reader = RuleBasedHandlingApi.GetReader(
            topLevelDirectory: appConfigRepoDeploy.RepositoryRoot,
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
        Success = BuildTarball(_reader, new FileInfo(AppConfigRepo.OutputFileTempPath), AppConfig.Targets, false);

        return Success;
    }

    public override void CleanUp()
    {
        try
        {
            if (Success && File.Exists(AppConfigRepo.OutputFileTempPath))
            {
                Log.Debug("Moving the archive file from the temporary folder to the target destination {TheTarget}", 
                    AppConfig.OutputFile);
                File.Move(AppConfigRepo.OutputFileTempPath, AppConfig.OutputFile.FullName);
            }
            
            AppEnv.TemporaryDirectory.Delete(true);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Failed to clean up");
        }
    }

    private AppConfigRepoDeploy AppConfig { get; }

    private readonly IRepositoryReader _reader;

}