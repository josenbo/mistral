namespace vigorule;

internal class RepositoryReader(RepositoryReadRequest request) : IRepositoryReader
{
    public event IRepositoryReader.BeforeApplyFileHandling? BeforeApplyFileHandlingEvent;
    public event IRepositoryReader.AfterApplyFileHandling? AfterApplyFileHandlingEvent;
    public event IRepositoryReader.BeforeApplyDirectoryHandling? BeforeApplyDirectoryHandlingEvent;
    public event IRepositoryReader.AfterApplyDirectoryHandling? AfterApplyDirectoryHandlingEvent;

    public IEnumerable<IFinalHandling> FilesAndDirectories => _transformations; 
    public IEnumerable<IFinalFileHandling> Files => _transformations.OfType<IFinalFileHandling>();
    public IEnumerable<IFinalDirectoryHandling> Directories => _transformations.OfType<IFinalDirectoryHandling>();
    
    public void Read()
    {
        _transformations.Clear();
        
        var controller = new DirectoryController(request.TopLevelDirectory, request);

        ProcessDirectory(controller);
    }
    
    private void ProcessDirectory(DirectoryController controller)
    {
        var mutableDirectoryHandling = controller.GetDirectoryTransformation();
        
        BeforeApplyDirectoryHandlingEvent?.Invoke(mutableDirectoryHandling);

        var finalDirectoryHandling = mutableDirectoryHandling.CheckAndTransform();
        
        AfterApplyDirectoryHandlingEvent?.Invoke(finalDirectoryHandling);
        
        if (finalDirectoryHandling.KeepEmptyDirectory)
            _transformations.Add(finalDirectoryHandling);
        
        foreach (var fi in controller.Location.EnumerateFiles())
        {
            var mutableFileHandling = controller.GetFileTransformation(fi);

            BeforeApplyFileHandlingEvent?.Invoke(mutableFileHandling);

            var finalFileHandling = mutableFileHandling.CheckAndTransform();
            
            AfterApplyFileHandlingEvent?.Invoke(finalFileHandling);
            
            if (finalFileHandling.CanDeploy)
                _transformations.Add(finalFileHandling);
        }

        if (!request.WalkFolderTree) 
            return;
        
        foreach (var di in controller.Location.EnumerateDirectories())
        {
            if (di.Name.Equals(".git", StringComparison.InvariantCultureIgnoreCase))
                continue;
            ProcessDirectory(new DirectoryController(di, request));
        }
    }

    private readonly List<IFinalHandling> _transformations = [];
}