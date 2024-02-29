using System.Diagnostics.CodeAnalysis;
using Serilog;
using vigoarchive;
using vigobase;

namespace vigo;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
internal class JobRunnerRepoDeploy : IJobRunner
{
    public IRepositoryReader RepositoryReader => _reader;
    public bool Success { get; private set; }

    public JobRunnerRepoDeploy(AppConfigRepoDeploy appConfigRepoDeploy)
    {
        AppConfig = appConfigRepoDeploy;
        AppEnv.TopLevelDirectory = AppConfig.RepositoryRoot; 
        _reader = new RepositoryReader(appConfigRepoDeploy);
    }
    
    public bool Prepare()
    {
        _reader.ReadRepository();
        
        return _reader.FileTransformations.All(ft => ft.CheckedSuccessfully) &&
               _reader.DirectoryTransformations.All(dt => dt.CheckedSuccessfully);
    }
    
    public bool Run()
    {
        Success = BuildTarball(_reader, AppConfig);

        return Success;
    }

    public void CleanUp()
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

    private readonly RepositoryReader _reader;

    private static bool BuildTarball(IRepositoryReader reader, AppConfigRepoDeploy appConfig)
    {
        try
        {
            var directoryTimestamp = DateTimeOffset.Now;
            
            var targets = reader
                .FileTransformations
                .SelectMany(ft => ft.DeploymentTargets)
                .Distinct()
                .ToList();

            var requests = new List<TarItem>();
            
            foreach (var target in targets)
            {
                // ReSharper disable LoopCanBeConvertedToQuery

                foreach (var transformation in reader.FileTransformations)
                {
                    requests.Add(new TarItemFile(
                        TarRelativePath: Path.Combine(target, AppEnv.GetTopLevelRelativePath(transformation.TargetFile)),
                        TransformedContent: transformation.CheckedAndTransformedTemporaryFile,
                        FileMode: transformation.FilePermission.ComputeUnixFileMode(AppEnv.DefaultFileHandlingParams.StandardModeForFiles),
                        ModificationTime: DateTimeOffset.FromFileTime(transformation.SourceFile.LastWriteTime.ToFileTime())
                        ));
                }

                foreach (var transformation in reader.DirectoryTransformations)
                {
                    requests.Add(new TarItemDirectory(
                        TarRelativePath: Path.Combine(target, AppEnv.GetTopLevelRelativePath(transformation.SourceDirectory)),
                        FileMode: AppEnv.DefaultFileHandlingParams.StandardModeForDirectories,
                        ModificationTime: directoryTimestamp
                        ));
                }
                
                // ReSharper restore LoopCanBeConvertedToQuery
            }

            var tarball = new Tarball()
            {
                DirectoryFileMode = AppEnv.DefaultFileHandlingParams.StandardModeForDirectories
            };

            foreach (var request in requests)
            {
                tarball.AddItem(request);
            }
        
            tarball.Save(appConfig.OutputFile);
            
            return true;
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to deploy repository files to a tarball. The tarball file might be incomplete and will be deleted, if it exists");
            Console.Error.WriteLine("Fatal: the program terminates prematurely due to an unhandled error");

            try
            {
                appConfig.OutputFile.Refresh();
                if (appConfig.OutputFile.Exists)
                    appConfig.OutputFile.Delete();
            }
            catch (Exception)
            {
                // ignore errors during cleanup
            }

            return false;
        }
    }
}