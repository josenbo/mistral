using JetBrains.Annotations;
using vigobase;

namespace vigocfg;

[PublicAPI]
public class VigoConfig : IVigoConfig
{
    public IFileSystemEnvironment FileSystemEnv { get; }
    // public INameParserConfig NameParserConfig { get; } = new NameParserConfig();
    public IEnumerable<IAppUser> AppUsers => _appUsers;
    // public IDatabaseScripts DatabaseScripts => _databaseScripts;
    // public IStagingEnvironment StagingEnvironment { get; } = new StagingEnvironmentImpl();

    public VigoConfig(Configuration? environmentSettings)
    {
        if (environmentSettings is null)
        {
            if (GuessSessionEnvironment.GuessAvailable)
                environmentSettings = GuessSessionEnvironment.GuessedEnvironmentSettings ??
                                      throw new NullReferenceException("Inconsistent data for the environment guess");
            else
                throw new ArgumentException("Need to provide environment settings as there is no default", nameof(environmentSettings));
        }

        // EnvironmentHelper.Staging = environmentSettings.StagingEnvironment;
        
        // if (environmentSettings.DefaultLineEndingOrNullForPlatformDefault.HasValue)
        //     EnvironmentHelper.DefaultLineEnding = environmentSettings.DefaultLineEndingOrNullForPlatformDefault.Value;
        
        FileSystemEnv = new FileSystemEnvironment(environmentSettings);

        _appUsers = new List<AppUser>()
        {
            new AppUser("cupines", FileSystemEnv.SourceRepositoryCupines, FileSystemEnv.TargetFolderCupines, true),
            new AppUser("stammaus", FileSystemEnv.SourceRepositoryStammaus, FileSystemEnv.TargetFolderStammaus, true)
        };

        var sourceFileProperties = new SourceFileProperties(
            FileTypeEnum.TextFile, 
            FileEncodingEnum.UTF_8);
        
        var targetFileProperties = new TargetFileProperties(
            FileEncodingEnum.UTF_8,
            EnvironmentHelper.DefaultLineEnding, 
            true, 
            FilePermission.UseDefault);

        // _databaseScripts = new DatabaseScripts(
        //     FileSystemEnv.SourceRepositoryFlyway,
        //     FileSystemEnv.TargetFolderFlyway,
        //     true,
        //     sourceFileProperties,
        //     targetFileProperties);
    }

    private readonly List<AppUser> _appUsers;
    // private readonly DatabaseScripts _databaseScripts;
}