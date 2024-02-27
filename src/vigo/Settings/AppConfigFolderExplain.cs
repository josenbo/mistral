using vigobase;

namespace vigo;

internal record AppConfigFolderExplain(
    FileInfo FolderConfiguration, 
    IReadOnlyCollection<string> Filenames
) : AppConfig
{
    public override CommandEnum Command => CommandEnum.Explain;
}