using vigobase;
// ReSharper disable NotAccessedPositionalProperty.Global

namespace vigo;

internal record AppConfigFolderExplain(
    FileInfo ConfigurationFile, 
    IReadOnlyList<string> Names
) : AppConfigFolder(ConfigurationFile.Directory ?? throw new VigoFatalException(
    AppEnv.Faults.Fatal(
        "FX511",
        "Configuration files are expected to reside in an existing folder. So this reference must never be null",
        string.Empty)))
{
    public override CommandEnum Command => CommandEnum.Explain;
}