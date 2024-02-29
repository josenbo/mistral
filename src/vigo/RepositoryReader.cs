using vigobase;
using vigorule;

namespace vigo;

internal class RepositoryReader : IRepositoryReader
{
    public event IRepositoryReader.BeforeApplyFileTransformation? BeforeApplyFileTransformationEvent;
    public event IRepositoryReader.BeforeApplyDirectoryTransformation? BeforeApplyDirectoryTransformationEvent;

    public IEnumerable<IDeploymentTransformationReadOnlyFile> FileTransformations =>
        _transformations.OfType<IDeploymentTransformationReadOnlyFile>();
    public IEnumerable<IDeploymentTransformationReadOnlyDirectory> DirectoryTransformations =>
        _transformations.OfType<IDeploymentTransformationReadOnlyDirectory>();
    
    public void ReadRepository()
    {
        Console.WriteLine($"Collecting files for deployment in the repository folder tree at {_appConfigRepo.RepositoryRoot}");
        
        var rules = new DirectoryController(_appConfigRepo.RepositoryRoot, AppEnv.DefaultFileHandlingParams);

        ProcessDirectory(rules);
    }
    
    internal RepositoryReader(AppConfigRepo appConfigRepo)
    {
        _appConfigRepo = appConfigRepo;
    }
    
    private void ProcessDirectory(DirectoryController controller)
    {
        var directoryTransformation = controller.GetDirectoryTransformation();
        
        BeforeApplyDirectoryTransformationEvent?.Invoke(directoryTransformation);

        var checkedDirectory = directoryTransformation.CheckAndTransform();
        
        if (checkedDirectory.KeepEmptyDirectory)
            _transformations.Add(checkedDirectory);
        
        foreach (var fi in controller.Location.EnumerateFiles())
        {
            var transformation = controller.GetFileTransformation(fi);

            BeforeApplyFileTransformationEvent?.Invoke(transformation);

            var checkedFile = transformation.CheckAndTransform();
            
            if (checkedFile.CanDeploy)
                _transformations.Add(checkedFile);
        }

        foreach (var di in controller.Location.EnumerateDirectories())
        {
            if (di.Name.Equals(".git", StringComparison.InvariantCultureIgnoreCase))
                continue;
            ProcessDirectory(new DirectoryController(di, controller.ParentFileHandlingParams));
        }
    }

    private readonly AppConfigRepo _appConfigRepo;
    private readonly List<IDeploymentTransformationReadOnly> _transformations = [];
}