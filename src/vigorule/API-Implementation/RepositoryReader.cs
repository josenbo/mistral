namespace vigorule;

internal class RepositoryReader(RepositoryReadRequest request) : IRepositoryReader
{
    public event IRepositoryReader.BeforeApplyFileHandling? BeforeApplyFileHandlingEvent;
    public event IRepositoryReader.AfterApplyFileHandling? AfterApplyFileHandlingEvent;
    public event IRepositoryReader.BeforeApplyDirectoryHandling? BeforeApplyDirectoryHandlingEvent;
    public event IRepositoryReader.AfterApplyDirectoryHandling? AfterApplyDirectoryHandlingEvent;

    public IEnumerable<IFinalHandling> FilesAndDirectories => _finalItems; 
    public IEnumerable<IFinalFileHandling> Files => _finalItems.OfType<IFinalFileHandling>();
    public IEnumerable<IFinalDirectoryHandling> Directories => _finalItems.OfType<IFinalDirectoryHandling>();

    IEnumerable<T> IRepositoryReader.FinalItems<T>(bool canDeployOnly) // where T : IFinalHandling
    {
        return canDeployOnly
            ? _finalItems.OfType<T>().Where(t => t.CanDeploy)
            : _finalItems.OfType<T>();
    }

    public void Read()
    {
        _finalItems.Clear();
        
        var controller = new DirectoryController(request.TopLevelDirectory, request);

        ProcessDirectory(controller);
    }
    
    private int ProcessDirectory(DirectoryController controller)
    {
        var deployableFileCount = 0;
        
        var mutableDirectoryHandling = controller.GetDirectoryTransformation();
        
        BeforeApplyDirectoryHandlingEvent?.Invoke(mutableDirectoryHandling);
        
        foreach (var fi in controller.Location.EnumerateFiles())
        {
            var mutableFileHandling = controller.GetFileTransformation(fi);

            BeforeApplyFileHandlingEvent?.Invoke(mutableFileHandling);

            var finalFileHandling = mutableFileHandling.CheckAndTransform();
            
            AfterApplyFileHandlingEvent?.Invoke(finalFileHandling);

            if (finalFileHandling.CanDeploy)
                deployableFileCount++;

            _finalItems.Add(finalFileHandling);
        }

        if (request.WalkFolderTree)
        {
            foreach (var di in controller.Location.EnumerateDirectories())
            {
                if (di.Name.Equals(".git", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                deployableFileCount += ProcessDirectory(new DirectoryController(di, request));
            }
        }

        mutableDirectoryHandling.IsEmptyDirectory = deployableFileCount == 0;
        mutableDirectoryHandling.CanDeploy = mutableDirectoryHandling.KeepEmptyDirectory || 0 < deployableFileCount;
        
        var finalDirectoryHandling = mutableDirectoryHandling.CheckAndTransform();
        
        AfterApplyDirectoryHandlingEvent?.Invoke(finalDirectoryHandling);
        
        _finalItems.Add(finalDirectoryHandling);

        return deployableFileCount;
    }

    private readonly List<IFinalHandling> _finalItems = [];
}