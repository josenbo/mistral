using vigoarchive;

namespace vigo;

internal class TarballBuilder
{
    public void Build()
    {
        var reader = new RepositoryReader(_appSettings);
        
        reader.ReadRepository();
        
        var tarball = new Tarball(_appSettings.RepositoryRoot.FullName, _appSettings.AdditionalTarRootFolder);

        foreach (var transformation in reader.FileTransformations)
        {
            tarball.AddFile(transformation.TargetFile);
        }
        
        tarball.Save(_appSettings.Tarball);
    }
    
    internal TarballBuilder(AppSettingsDeployToTarball appSettings)
    {
        _appSettings = appSettings;
    }


    private readonly AppSettingsDeployToTarball _appSettings;
}