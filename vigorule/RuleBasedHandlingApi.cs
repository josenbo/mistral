using JetBrains.Annotations;
using vigobase;

namespace vigorule;

[PublicAPI]
public static class RuleBasedHandlingApi
{
    public static IRepositoryReader GetReader
    (
        DirectoryInfo topLevelDirectory, 
        FileHandlingParameters defaultHandling,
        IReadOnlyList<ConfigurationFilename> configFiles
    )
    {
        var request = new RepositoryReadRequest(
            TopLevelDirectory: topLevelDirectory,
            ConfigFiles: configFiles,
            WalkFolderTree: true,
            DefaultHandling: defaultHandling);
        
        return new RepositoryReader(request);
    }

    public static IRepositoryReader GetReader
    (
        FileInfo configFile,
        FileHandlingParameters defaultHandling
    )
    {
        var request = new RepositoryReadRequest(
            TopLevelDirectory: configFile.Directory ?? throw new VigoFatalException(AppEnv.Faults.Fatal(
                "FX112", 
                "File is expected to be already checked for existence, so this reference will not be null", 
                string.Empty)),
            ConfigFiles: new []{ new ConfigurationFilename(configFile.Name, ConfigurationFileTypeEnum.Undefined)},
            WalkFolderTree: false,
            DefaultHandling: defaultHandling);

        return new RepositoryReader(request);
    }
}