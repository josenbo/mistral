using vigobase;

namespace vigo;

internal record AppConfigInfoHelp(CommandEnum CommandToShowHelpFor) : AppConfigInfo
{
    public override CommandEnum Command => CommandEnum.Help;
}