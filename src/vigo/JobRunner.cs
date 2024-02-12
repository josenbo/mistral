using System.Diagnostics.CodeAnalysis;
using Serilog;
using vigoarchive;
using vigobase;

namespace vigo;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
internal class JobRunner(AppSettings settings)
{
    public IRepositoryReader RepositoryReader => _reader;
    private AppSettings Settings { get; } = settings;
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
            Success = Settings switch
            {
                AppSettingsDeployToTarball configurationDeployToTarball => BuildTarball(_reader, configurationDeployToTarball),
                AppSettingsCheckCommit configurationCheckCommit => RunCommitChecks(_reader, configurationCheckCommit),
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
            if (Success && Settings is AppSettingsDeployToTarball configTarball && File.Exists(configTarball.TemporaryTarballPath))
                File.Move(configTarball.TemporaryTarballPath, configTarball.Tarball.FullName);
            
            Settings.TemporaryDirectory.Delete(true);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Failed to clean up");
        }
    }

    private readonly RepositoryReader _reader = new RepositoryReader(settings);
    
    private static bool RunCommitChecks(RepositoryReader reader, AppSettingsCheckCommit config)
    {
        return true;
    }

    private static bool BuildTarball(RepositoryReader reader, AppSettingsDeployToTarball config)
    {
        try
        {
            var tarball = new Tarball(config.RepositoryRoot.FullName, config.AdditionalTarRootFolder);

            foreach (var transformation in reader.FileTransformations)
            {
                tarball.AddFile(transformation.TargetFile);
            }
        
            tarball.Save(config.Tarball);
            
            return true;
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to deploy repository files to a tarball. The tarball file might be incomplete and will be deleted, if it exists");
            Console.Error.WriteLine("Fatal: the program terminates prematurely due to an unhandled error");

            try
            {
                config.Tarball.Refresh();
                if (config.Tarball.Exists)
                    config.Tarball.Delete();
            }
            catch (Exception)
            {
                // ignore errors during cleanup
            }

            return false;
        }
    }
}