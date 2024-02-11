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
        
        var rules = new DirectoryController(_appSettings.RepositoryRoot, _appSettings.DefaultFileHandlingParams);

        ProcessDirectory(rules);
    }
    
    internal RepositoryReader(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }
    
    private void ProcessDirectory(DirectoryController controller)
    {
        var directoryTransformation = controller.GetDirectoryTransformation();
        
        BeforeApplyDirectoryTransformationEvent?.Invoke(directoryTransformation);
        
        directoryTransformation.CheckAndTransform();
        
        if (directoryTransformation.KeepEmptyDirectory)
            _transformations.Add(directoryTransformation.GetReadOnlyInterface());
        
        foreach (var fi in controller.Location.EnumerateFiles())
        {
            var transformation = controller.GetFileTransformation(fi);

            BeforeApplyFileTransformationEvent?.Invoke(transformation);

            transformation.CheckAndTransform();
            
            if (transformation.CanDeploy)
                _transformations.Add(transformation.GetReadOnlyInterface());
        }

        foreach (var di in controller.Location.EnumerateDirectories())
        {
            if (di.Name.Equals(".git", StringComparison.InvariantCultureIgnoreCase))
                continue;
            ProcessDirectory(new DirectoryController(di, controller.ParentFileHandlingParams));
        }
    }

    private readonly AppSettings _appSettings;
    private readonly List<IDeploymentTransformationReadOnly> _transformations = [];
}