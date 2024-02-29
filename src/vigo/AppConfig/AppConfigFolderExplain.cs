using vigobase;
// ReSharper disable NotAccessedPositionalProperty.Global

namespace vigo;

internal record AppConfigFolderExplain(
    FileInfo ConfigurationFile, 
    IReadOnlyList<string> Names
) : AppConfigFolder(ConfigurationFile.Directory ?? throw new VigoFatalException($"No parent directory for the folder configuration {ConfigurationFile.FullName}"))
{
    public override CommandEnum Command => CommandEnum.Explain;
}