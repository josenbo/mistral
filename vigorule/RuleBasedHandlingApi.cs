using JetBrains.Annotations;
using vigobase;

namespace vigorule;

[PublicAPI]
public static class RuleBasedHandlingApi
{
    public static IRepositoryReader GetReader
    (
        DirectoryInfo topLevelDirectory,
        bool includePreview,
        bool onlyTopLeveDirectory,
        FileHandlingParameters defaultHandling,
        IReadOnlyList<ConfigurationFilename> configFiles
    )
    {
        var request = new RepositoryReadRequest(
            TopLevelDirectory: topLevelDirectory,
            ConfigFiles: configFiles,
            IncludePreview: includePreview,
            OnlyTopLevelDirectory: onlyTopLeveDirectory,
            DefaultHandling: defaultHandling);
        
        return new RepositoryReader(request);
    }
}