using Serilog;
using Serilog.Events;
using vigobase;

namespace vigo;

internal record AppConfigVersion : AppConfig
{
    public override CommandEnum Command => CommandEnum.Version;
    
    public override void LogObject()
    {
        if (!Log.IsEnabled(LogEventLevel.Debug))
            return;
        
        Log.Debug("Dumping an object of type {TheType}", this.GetType().FullName);
        Log.Debug("{TheParam} = {TheValue}", nameof(Command), Command);
    }
}