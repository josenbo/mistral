using vigoarchive;

namespace vigo;

internal class TarballBuilder
{
    public void Build()
    {
        var reader = new RepositoryReader(_configuration);
        
        reader.ReadRepository();
        
        var tarball = new Tarball(_configuration.RepositoryRoot.FullName, _configuration.AdditionalTarRootFolder);

        foreach (var transformation in reader.Transformations)
        {
            tarball.AddFile(transformation.TargetFile);
        }
        
        tarball.Save(_configuration.Tarball);
    }
    
    internal TarballBuilder(ConfigurationDeployToTarball configuration)
    {
        _configuration = configuration;
    }


    private readonly ConfigurationDeployToTarball _configuration;
}