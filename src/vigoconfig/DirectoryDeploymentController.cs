using vigobase;

namespace vigoconfig;

public class DirectoryDeploymentController
{
    public DeploymentDefaults Defaults { get; }
    public DirectoryInfo Location { get; }

    public IDeploymentTransformationReadWrite GetFileTransformation(FileInfo file)
    {
        var transformation = DeploymentTransformationFactory.Create(file, _defaults);
        transformation.CanDeploy = HasDeploymentRules && file.Exists && file.Name != _defaults.DeploymentConfigFileName;
        return transformation;
    }

    public DirectoryDeploymentController(DirectoryInfo directory, DeploymentDefaults defaults)
    {
        _defaults = defaults;
        Defaults = defaults;
        Location = directory;
        _folderConfig = FolderConfigFactory.Create(directory, defaults);
        HasDeploymentRules = File.Exists(Path.Combine(directory.FullName, _defaults.DeploymentConfigFileName));
    }

    private bool HasDeploymentRules { get; }
    
    private readonly DeploymentDefaults _defaults;
    private readonly FolderConfig _folderConfig;


}
