using vigobase;

namespace vigocfg;

public interface IVigoConfig
{
    IFileSystemEnvironment FileSystemEnv { get; }
    IEnumerable<IAppUser> AppUsers { get; }
    // DatabaseScripts DatabaseScripts { get; }
    IEnvironmentDescriptor EnvironmentDescriptor { get; }
}