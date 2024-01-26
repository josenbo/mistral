using vigobase;

namespace vigoconfig;

public class DeploymentRules
{
    public DeploymentDefaults CurrentDefaults { get; }
    public DirectoryInfo CurrentDirectory { get; }
    public bool DirectoryHasDeploymentRules { get; }

    public bool DeployFile(FileInfo file)
    {
        return DirectoryHasDeploymentRules && file.Exists && file.Name != _defaults.DeploymentConfigFileName;
    }
    
    public DeploymentRules(DirectoryInfo directory, DeploymentDefaults defaults)
    {
        _defaults = defaults;
        CurrentDefaults = defaults;
        CurrentDirectory = directory;
        DirectoryHasDeploymentRules = File.Exists(Path.Combine(directory.FullName, _defaults.DeploymentConfigFileName));
    }

    private readonly DeploymentDefaults _defaults;
}