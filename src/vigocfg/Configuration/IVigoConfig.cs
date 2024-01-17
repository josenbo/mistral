namespace vigocfg;

public interface IVigoConfig
{
    IFileSystemEnvironment FileSystemEnv { get; }
    INameParserConfig NameParserConfig { get; }
    IEnumerable<IAppUser> AppUsers { get; }
    IDatabaseScripts DatabaseScripts { get; }
    IStagingEnvironment StagingEnvironment { get; }
}