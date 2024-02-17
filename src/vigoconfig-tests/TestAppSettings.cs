namespace vigoconfig_tests;

internal class TestAppSettings : IAppSettings
{
    public CommandEnum Command => throw new NotImplementedException();

    public DirectoryInfo RepositoryRoot => throw new NotImplementedException();

    public string DeploymentConfigFileName => throw new NotImplementedException();

    public FileHandlingParameters DefaultFileHandlingParams => throw new NotImplementedException();

    public StandardFileHandling DeployConfigRule => throw new NotImplementedException();

    public StandardFileHandling FinalCatchAllRule => throw new NotImplementedException();

    public string GetRepoRelativePath(string path)
    {
        throw new NotImplementedException();
    }

    public string GetRepoRelativePath(FileSystemInfo file)
    {
        throw new NotImplementedException();
    }

    public string GetTemporaryFilePath()
    {
        throw new NotImplementedException();
    }
}