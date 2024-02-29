using Serilog;
using vigobase;

namespace vigo;

internal class JobRunnerRepoCheck(AppConfigRepoCheck appConfigRepoCheck) : IJobRunner
{
    public IRepositoryReader RepositoryReader => _reader;
    private AppConfigRepoCheck AppConfig { get; } = appConfigRepoCheck;
    public bool Success { get; private set; }

    public bool Prepare()
    {
        _reader.ReadRepository();
        
        return _reader.FileTransformations.All(ft => ft.CheckedSuccessfully) &&
               _reader.DirectoryTransformations.All(dt => dt.CheckedSuccessfully);
    }

    public bool Run()
    {
        Success = RunCommitChecks(_reader, AppConfig);
        return Success;
    }

    public void CleanUp()
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

    private readonly RepositoryReader _reader = new RepositoryReader(appConfigRepoCheck);

    private static bool RunCommitChecks(IRepositoryReader reader, AppConfigRepoCheck appConfig)
    {
        return true;
    }
}