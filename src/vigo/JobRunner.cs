using System.Diagnostics.CodeAnalysis;
using Serilog;
using vigoarchive;
using vigobase;

namespace vigo;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
internal class JobRunner(AppConfigRepo configRepo)
{
    public IRepositoryReader RepositoryReader => _reader;
    private AppConfigRepo ConfigRepo { get; } = configRepo;
    public bool Success { get; private set; }
    
    public bool Prepare()
    {
        try
        {
            _reader.ReadRepository();
            
            return _reader.FileTransformations.All(ft => ft.CheckedSuccessfully) &&
                   _reader.DirectoryTransformations.All(dt => dt.CheckedSuccessfully);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Scanning and processing the repository failed");
            return false;
        }
    }
    
    public bool Run()
    {
        try
        {
            Success = ConfigRepo switch
            {
                AppConfigRepoDeploy configurationDeployToTarball => BuildTarball(_reader, configurationDeployToTarball),
                AppConfigRepoCheck configurationCheckCommit => RunCommitChecks(_reader, configurationCheckCommit),
                _ => false
            };
        }
        catch(Exception e)
        {
            // ignored
            Log.Fatal(e, "Preparing the deployment failed");
            Success = false;
        }

        return Success;
    }

    public void CleanUp()
    {
        try
        {
            if (Success && ConfigRepo is AppConfigRepoDeploy configTarball && File.Exists(AppConfigRepo.TemporaryTarballPath))
                File.Move(AppConfigRepo.TemporaryTarballPath, configTarball.Tarball.FullName);
            
            AppEnv.TemporaryDirectory.Delete(true);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Failed to clean up");
        }
    }

    private readonly RepositoryReader _reader = new RepositoryReader(configRepo);
    
    private static bool RunCommitChecks(RepositoryReader reader, AppConfigRepoCheck configRepo)
    {
        return true;
    }

    private static bool BuildTarball(RepositoryReader reader, AppConfigRepoDeploy configRepo)
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
        
            tarball.Save(configRepo.Tarball);
            
            return true;
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to deploy repository files to a tarball. The tarball file might be incomplete and will be deleted, if it exists");
            Console.Error.WriteLine("Fatal: the program terminates prematurely due to an unhandled error");

            try
            {
                configRepo.Tarball.Refresh();
                if (configRepo.Tarball.Exists)
                    configRepo.Tarball.Delete();
            }
            catch (Exception)
            {
                // ignore errors during cleanup
            }

            return false;
        }
    }
}