using vigobase;
using vigoconfig;

namespace vigo;

internal class RepositoryReader
{
    public delegate void BeforeApplyTransformation(IDeploymentTransformationReadWrite transformation);

    public IEnumerable<IDeploymentTransformationReadOnly> Transformations => _transformations;

    public event BeforeApplyTransformation? BeforeApplyTransformationEvent;
    
    public void ReadRepository()
    {
        var defaults = GetDeploymentDefaults(_configuration.DeploymentConfigFileName);
        var rules = new DirectoryDeploymentController(_configuration.RepositoryRoot, defaults);

        ProcessDirectory(rules);
    }
    
    internal RepositoryReader(Configuration configuration)
    {
        _configuration = configuration;
    }
    
    private void ProcessDirectory(DirectoryDeploymentController controller)
    {
        foreach (var fi in controller.Location.EnumerateFiles())
        {
            var transformation = controller.GetFileTransformation(fi);

            BeforeApplyTransformationEvent?.Invoke(transformation);

            if (transformation.CanDeploy)
                _transformations.Add(transformation.GetReadOnlyInterface());
        }

        foreach (var di in controller.Location.EnumerateDirectories())
        {
            if (di.Name.Equals(".git", StringComparison.InvariantCultureIgnoreCase))
                continue;
            ProcessDirectory(new DirectoryDeploymentController(di, controller.Defaults));
        }
    }
    
    private static DeploymentDefaults GetDeploymentDefaults(string deploymentConfigFileName)
    {
        return new DeploymentDefaults(
            DeploymentConfigFileName: deploymentConfigFileName,
            FileModeDefault: (UnixFileMode)0b_110_110_100,
            DirectoryModeDefault: (UnixFileMode)0b_111_111_101,
            SourceFileEncodingDefault: FileEncodingEnum.UTF_8,
            TargetFileEncodingDefault: FileEncodingEnum.UTF_8,
            LineEndingDefault: LineEndingEnum.LF,
            TrailingNewlineDefault: true,
            FileTypeDefault: FileTypeEnum.TextFile    
        );
    }

    private readonly Configuration _configuration;
    private readonly List<IDeploymentTransformationReadOnly> _transformations = [];
}