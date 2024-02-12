namespace vigobase;

public interface IAppSettings
{
    CommandEnum Command { get; }
    DirectoryInfo RepositoryRoot { get; }
    string DeploymentConfigFileName { get; }
    FileHandlingParameters DefaultFileHandlingParams { get; }
    StandardFileHandling DeployConfigRule { get; } 
    StandardFileHandling FinalCatchAllRule { get; } 
    string GetRepoRelativePath(string path);
    string GetRepoRelativePath(FileSystemInfo file);
    string GetTemporaryFilePath();
}