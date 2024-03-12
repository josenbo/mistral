using Serilog;
using vigobase;

namespace vigorule;

internal class RepositoryReader(RepositoryReadRequest request) : IRepositoryReader
{
    public event IRepositoryReader.BeforeApplyFileHandling? BeforeApplyFileHandlingEvent;
    public event IRepositoryReader.AfterApplyFileHandling? AfterApplyFileHandlingEvent;
    public event IRepositoryReader.BeforeApplyDirectoryHandling? BeforeApplyDirectoryHandlingEvent;
    public event IRepositoryReader.AfterApplyDirectoryHandling? AfterApplyDirectoryHandlingEvent;

    public DirectoryInfo TopLevelDirectory => request.TopLevelDirectory;
    public string GetTopLevelRelativePath(string path) => request.GetTopLevelRelativePath(path);
    public string GetTopLevelRelativePath(FileSystemInfo fileSystemItem) => request.GetTopLevelRelativePath(fileSystemItem);
    public FileHandlingParameters DefaultHandling => request.DefaultHandling;
    
    IEnumerable<T> IRepositoryReader.FinalItems<T>(bool canDeployOnly) // where T : IFinalHandling
    {
        return canDeployOnly
            ? _finalItems.OfType<T>().Where(t => t.CanDeploy)
            : _finalItems.OfType<T>();
    }

    IEnumerable<T> IRepositoryReader.FinalItems<T>(string target) // where T : IFinalHandling
    {
        return _finalItems.OfType<T>().Where(t => t.CanDeploy && t.HasDeploymentTarget(target));
    }

    public IEnumerable<string> Targets()
    {
        if (0 < _targets.Count || _finalItems.Count == 0)
            return _targets;

        _targets.AddRange(
            _finalItems
                .Where(t => t.CanDeploy)
                .SelectMany(i => i.DeploymentTargets)
                .Distinct()
            );
        
        return _targets;
    }

    public void Read()
    {
        _finalItems.Clear();
        _targets.Clear();
        
        var controller = new DirectoryController(request.TopLevelDirectory, request, true);

        ProcessDirectory(controller);
    }
    
    private int ProcessDirectory(DirectoryController controller)
    {
        var deployableFileCount = 0;
        
        var mutableDirectoryHandling = controller.GetDirectoryTransformation();
        
        BeforeApplyDirectoryHandlingEvent?.Invoke(mutableDirectoryHandling);
        
        foreach (var fi in controller.Location.EnumerateFiles())
        {
            // if (fi.Name.Equals("tia_ines_daten.sql"))
            //     Log.Debug("Arrived at {TheFile}", fi);
            
            var mutableFileHandling = controller.GetFileTransformation(fi);

            BeforeApplyFileHandlingEvent?.Invoke(mutableFileHandling);

            var finalFileHandling = mutableFileHandling.CheckAndTransform();
            
            AfterApplyFileHandlingEvent?.Invoke(finalFileHandling);

            if (finalFileHandling.CanDeploy)
            {
                deployableFileCount++;
                controller.CollectDeploymentTargets(finalFileHandling.DeploymentTargets);                
            }

            _finalItems.Add(finalFileHandling);
        }

        if (request.WalkFolderTree)
        {
            foreach (var di in controller.Location.EnumerateDirectories())
            {
                if (di.Name.Equals(".git", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var subDirController = new DirectoryController(di, request, false);
                deployableFileCount += ProcessDirectory(subDirController);
                controller.CollectDeploymentTargets(subDirController.GetTargets());
            }
        }

        mutableDirectoryHandling.CanDeploy = !controller.IsTopLevelDirectory && (mutableDirectoryHandling.KeepEmptyDirectory || 0 < deployableFileCount);
        
        var finalDirectoryHandling = mutableDirectoryHandling.CheckAndTransform();
        
        AfterApplyDirectoryHandlingEvent?.Invoke(finalDirectoryHandling);
        
        _finalItems.Add(finalDirectoryHandling);

        return deployableFileCount;
    }

    private readonly List<IFinalHandling> _finalItems = [];
    private readonly List<string> _targets = [];
}