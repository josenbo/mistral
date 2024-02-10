namespace vigobase;

public interface IAppSettings
{
    DirectoryInfo RepositoryRoot { get; }
    string DeploymentConfigFileName { get; }
    FileHandlingParameters DefaultFileHandlingParams { get; }
    bool ImplicitFinalRuleIsSkippingTheFile { get;  }
    FileHandlingParameters ImplicitFinalRuleHandling { get; }
    bool IsDeploymentRun { get; }
    bool IsCommitCheck { get; }
    string GetRepoRelativePath(string path);
    string GetRepoRelativePath(FileSystemInfo file);
}