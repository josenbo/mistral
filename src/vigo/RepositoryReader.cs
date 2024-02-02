using Serilog;
using vigobase;
using vigoconfig;

namespace vigo;

internal class RepositoryReader
{
    public delegate void BeforeApplyFileTransformation(IDeploymentTransformationReadWriteFile transformation);
    public delegate void BeforeApplyDirectoryTransformation(IDeploymentTransformationReadWriteDirectory transformation);

    public event BeforeApplyFileTransformation? BeforeApplyFileTransformationEvent;
    public event BeforeApplyDirectoryTransformation? BeforeApplyDirectoryTransformationEvent;

    public IEnumerable<IDeploymentTransformationReadOnlyFile> FileTransformations =>
        _transformations.OfType<IDeploymentTransformationReadOnlyFile>();
    public IEnumerable<IDeploymentTransformationReadOnlyDirectory> DirectoryTransformations =>
        _transformations.OfType<IDeploymentTransformationReadOnlyDirectory>();
    
    public void ReadRepository()
    {
        Log.Information("Scanning the repository folder tree");
        
        var defaults = GetDeploymentDefaults(
            _configuration.DeploymentConfigFileName, 
            _configuration.RepositoryRoot.FullName);
        
        var rules = new DirectoryController(_configuration.RepositoryRoot, defaults);

        ProcessDirectory(rules);
    }
    
    internal RepositoryReader(Configuration configuration)
    {
        _configuration = configuration;
    }
    
    private void ProcessDirectory(DirectoryController controller)
    {
        var directoryTransformation = controller.GetDirectoryTransformation();
        
        BeforeApplyDirectoryTransformationEvent?.Invoke(directoryTransformation);
        
        if (directoryTransformation.KeepEmptyDirectory)
            _transformations.Add(directoryTransformation.GetReadOnlyInterface());
        
        foreach (var fi in controller.Location.EnumerateFiles())
        {
            var transformation = controller.GetFileTransformation(fi);

            BeforeApplyFileTransformationEvent?.Invoke(transformation);

            if (transformation.CanDeploy)
                _transformations.Add(transformation.GetReadOnlyInterface());
        }

        foreach (var di in controller.Location.EnumerateDirectories())
        {
            if (di.Name.Equals(".git", StringComparison.InvariantCultureIgnoreCase))
                continue;
            ProcessDirectory(new DirectoryController(di, controller.Defaults));
        }
    }
    
    private static DeploymentDefaults GetDeploymentDefaults(string deploymentConfigFileName, string repositoryPath)
    {
        return new DeploymentDefaults(
            RepositoryPath: repositoryPath,
            DeploymentConfigFileName: deploymentConfigFileName,
            ValidCharachters: @"\u0000-\u007FäöüÄÖÜß€",
            FileModeDefault: (UnixFileMode)0b_110_110_100,
            DirectoryModeDefault: (UnixFileMode)0b_111_111_101,
            SourceFileEncodingDefault: FileEncodingEnum.UTF_8,
            TargetFileEncodingDefault: FileEncodingEnum.UTF_8,
            LineEndingDefault: LineEndingEnum.LF,
            TrailingNewlineDefault: true,
            FileTypeDefault: FileTypeEnum.BinaryFile    
        );
    }

    private readonly Configuration _configuration;
    private readonly List<IDeploymentTransformationReadOnly> _transformations = [];
}