using System.Diagnostics.CodeAnalysis;
using Serilog;

namespace vigo;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
internal class JobRunner(AppSettings settings)
{
    private AppSettings Settings { get; } = settings;
    public bool Success { get; private set; }
    
    public bool Prepare()
    {
        return true;
    }
    
    public bool Run()
    {
        try
        {
            Success = Settings switch
            {
                AppSettingsDeployToTarball configurationDeployToTarball => BuildTarball(configurationDeployToTarball),
                AppSettingsCheckCommit configurationCheckCommit => RunCommitChecks(configurationCheckCommit),
                _ => false
            };
        }
        finally
        {
            try
            {
                if (Settings is AppSettingsDeployToTarball configTarball && File.Exists(configTarball.TemporaryTarballPath))
                    File.Move(configTarball.TemporaryTarballPath, configTarball.Tarball.FullName);
            
                Settings.TemporaryDirectory.Delete(true);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Could not delete the temporary folder ({e.GetType().Name}: {e.Message})");
            }
        }

        return Success;
    }

    public void CleanUp()
    {
    }
    
    private static bool RunCommitChecks(AppSettingsCheckCommit config)
    {
        return true;
    }

    private static bool BuildTarball(AppSettingsDeployToTarball config)
    {
        try
        {
            var builder = new TarballBuilder(config);
    
            builder.Build();

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