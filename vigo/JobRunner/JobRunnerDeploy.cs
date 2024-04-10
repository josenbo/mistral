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
        if (AppConfig.DeploymentBundle is not null && AppConfig.DeploymentBundle.Exists)
            AppConfig.DeploymentBundle.Delete();
        
        if (AppConfig.MappingReport is not null && AppConfig.MappingReport.Exists)
            AppConfig.MappingReport.Delete();
        
        _reader.Read();

        return _reader
            .FinalItems<IFinalHandling>(true)
            .All(ft => ft.CheckedSuccessfully);
    }
    
    protected override bool DoRun()
    {
        if (!BuildTarball(_reader, AppEnv.TemporaryDeploymentBundle, AppConfig.Targets, false))
            return false;

        if (HasGhostFiles(_reader.TopLevelDirectory))
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
    
    private static bool HasGhostFiles(DirectoryInfo tld)
    {
        var retval = false;
        
        var query = tld.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(w => w.DirectoryName is not null)
                .Select(p => new
                {
                    FilePath = Path.GetRelativePath(tld.FullName, p.FullName),
                    DirPath = Path.GetRelativePath(tld.FullName, p.DirectoryName ?? string.Empty),
                    FileName = p.Name
                })
                .Where(c => !c.DirPath.StartsWith(".git"))
                .GroupBy(g => g.FilePath, StringComparer.InvariantCultureIgnoreCase)
                .Select(d => new 
                {
                    Key = d.Key,
                    Items = d
                })
                .Where(n => n.Items.Count() != 1);

        foreach (var item in query)
        {
            retval = true;
            
            Log.Error("Ghost files detected in folder {FolderPath}: [{GhostFiles}]", 
                Path.GetDirectoryName(item.Key),
                string.Join(", ", item.Items.Select(n => n.FileName)));
        }

        if (retval)
            AppEnv.Faults.Fatal("FX693", null,
                "Found ghost files with identical file names except for upper-case or lower-case letters. See log for file locations.");

        return retval;
    }
    
    private AppConfigDeploy AppConfig { get; }

    private readonly IRepositoryReader _reader;
}