using System.Diagnostics.CodeAnalysis;
using Serilog;
using vigoarchive;
using vigobase;
using vigorule;

namespace vigo;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
internal class JobRunnerRepoDeploy : JobRunner
{
    public JobRunnerRepoDeploy(AppConfigRepoDeploy appConfigRepoDeploy)
    {
        AppConfig = appConfigRepoDeploy;
        AppEnv.TopLevelDirectory = AppConfig.RepositoryRoot; 
        _reader = RuleBasedHandlingApi.GetReader(
            topLevelDirectory: appConfigRepoDeploy.RepositoryRoot,
            defaultHandling: AppEnv.DefaultFileHandlingParams,
            configFiles: AppEnv.DeployConfigRule.Filenames);
    }
    
    public override bool Prepare()
    {
        _reader.Read();
        
        return _reader
            .FinalItems<IFinalHandling>(true)
            .All(ft => ft.CheckedSuccessfully);
    }
    
    public override bool Run()
    {
        Success = BuildTarball(_reader, AppConfig.OutputFile);

        return Success;
    }

    public override void CleanUp()
    {
        try
        {
            if (Success && File.Exists(AppConfigRepo.OutputFileTempPath))
                File.Move(AppConfigRepo.OutputFileTempPath, AppConfig.OutputFile.FullName);
            
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