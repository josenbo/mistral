using System.Text.RegularExpressions;
using vigobase;

namespace vigoconfig;

public class DirectoryDeploymentController
{
    public DeploymentDefaults Defaults { get; }
    public DirectoryInfo Location { get; }
    public bool HasDeploymentRules { get; }

    public bool DeployFile(FileInfo file)
    {
        return HasDeploymentRules && file.Exists && file.Name != _defaults.DeploymentConfigFileName;
    }
    
    public DirectoryDeploymentController(DirectoryInfo directory, DeploymentDefaults defaults)
    {
        _defaults = defaults;
        Defaults = defaults;
        Location = directory;
        HasDeploymentRules = File.Exists(Path.Combine(directory.FullName, _defaults.DeploymentConfigFileName));
    }

    private readonly DeploymentDefaults _defaults;
}
