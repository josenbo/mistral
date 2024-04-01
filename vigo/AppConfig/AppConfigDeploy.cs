using Serilog;
using Serilog.Events;
using vigobase;

namespace vigo;

internal record AppConfigDeploy(
    DirectoryInfo RepositoryRoot,
    FileInfo? DeploymentBundle,
    IReadOnlyList<string> Targets,
    bool Preview,
    FileInfo? MappingReport
) : AppConfig
{

    public override CommandEnum Command => CommandEnum.Deploy;
    public static string OutputFileTempPath => Path.Combine(AppEnv.TemporaryDirectory.FullName, "vigo.tar.gz");
    public override void LogObject()
    {
        if (!Log.IsEnabled(LogEventLevel.Debug))
            return;
        
        Log.Debug("Dumping an object of type {TheType}", this.GetType().FullName);
        Log.Debug("{TheParam} = {TheValue}", nameof(Command), Command);
        Log.Debug("{TheParam} = {TheValue}", nameof(RepositoryRoot), RepositoryRoot);
        Log.Debug("{TheParam} = {TheValue}", nameof(DeploymentBundle), DeploymentBundle);
        Log.Debug("{TheParam} = {TheValue}", nameof(Targets), Targets);
        Log.Debug("{TheParam} = {TheValue}", nameof(MappingReport), MappingReport);
        Log.Debug("{TheParam} = {TheValue}", nameof(Preview), Preview);
    }

    public override string ToString()
    {
        return $"{nameof(AppConfigDeploy)} {{ {nameof(Command)} = {Command}, {nameof(RepositoryRoot)} = {RepositoryRoot}, {nameof(DeploymentBundle)} = {DeploymentBundle}, {nameof(Targets)} = [{string.Join(", ", Targets)}], {nameof(Preview)} = {Preview}, {nameof(MappingReport)} = {MappingReport} }}";
    }
}