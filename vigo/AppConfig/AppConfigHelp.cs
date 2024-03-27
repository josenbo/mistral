using Serilog;
using Serilog.Events;
using vigobase;

namespace vigo;

internal record AppConfigHelp() : AppConfig
{
    public override CommandEnum Command => CommandEnum.Help;
    public override void LogObject()
    {
        if (!Log.IsEnabled(LogEventLevel.Debug))
            return;
        
        Log.Debug("Dumping an object of type {TheType}", this.GetType().FullName);
        Log.Debug("{TheParam} = {TheValue}", nameof(Command), Command);
    }

    public override string ToString()
    {
        return $"{nameof(AppConfigHelp)} {{ {nameof(Command)} = {Command} }}";
    }
}