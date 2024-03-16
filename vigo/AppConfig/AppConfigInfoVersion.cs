using vigobase;

namespace vigo;

internal record AppConfigInfoVersion : AppConfigInfo
{
    public override CommandEnum Command => CommandEnum.Version;
}