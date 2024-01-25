using vigoarchive;

namespace vigo;

internal class TarballBuilder
{
    public void Build()
    {
        var tarball = new Tarball(_configuration.RepositoryRoot.FullName, _configuration.AdditionalTarRootFolder);
        
        foreach (var di in EnumerateDirectoriesWithDeploymentConfiguration(_configuration.RepositoryRoot, _configuration.DeploymentConfigFileName))
        {
            
        }
        
        tarball.Save(_configuration.Tarball);
    }
    
    internal TarballBuilder(Configuration configuration)
    {
        _configuration = configuration;
    }
    
    private static IEnumerable<DirectoryInfo> EnumerateDirectoriesWithDeploymentConfiguration(DirectoryInfo baseDir, string deploymentConfigFileName)
    {
        foreach (var fi in baseDir.EnumerateFiles(deploymentConfigFileName, SearchOption.AllDirectories))
        {
            if (fi.Directory is not null && CanProcessFilesInPath(fi.Directory))
            {
                yield return fi.Directory;
            }
        }
    }

    private static bool CanProcessFilesInPath(DirectoryInfo? directoryInfo)
    {
        var cd = directoryInfo;
        
        while (cd is not null)
        {
            if (cd.Name.Equals(".git", StringComparison.InvariantCultureIgnoreCase))
                return false;
            cd = cd.Parent;
        }

        return true;
    }

    private readonly Configuration _configuration;
}