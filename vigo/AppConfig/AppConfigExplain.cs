using Serilog;
using Serilog.Events;
using vigobase;

namespace vigo;

internal record AppConfigExplain(
    DirectoryInfo RepositoryRoot,
    string ExplainName
) : AppConfig
{
    public override CommandEnum Command => CommandEnum.Explain;
    public override void LogObject()
    {
        if (!Log.IsEnabled(LogEventLevel.Debug))
            return;
        
        Log.Debug("Dumping an object of type {TheType}", this.GetType().FullName);
        Log.Debug("{TheParam} = {TheValue}", nameof(Command), Command);
        Log.Debug("{TheParam} = {TheValue}", nameof(RepositoryRoot), RepositoryRoot);
        Log.Debug("{TheParam} = {TheValue}", nameof(ExplainName), ExplainName);
    }
}