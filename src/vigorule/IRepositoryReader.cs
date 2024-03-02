using vigobase;

namespace vigorule;

public interface IRepositoryReader
{
    delegate void BeforeApplyFileHandling(IMutableFileHandling transformation);
    delegate void BeforeApplyDirectoryHandling(IMutableDirectoryHandling transformation);
    delegate void AfterApplyFileHandling(IFinalFileHandling transformation);
    delegate void AfterApplyDirectoryHandling(IFinalDirectoryHandling transformation);
    
    event BeforeApplyFileHandling? BeforeApplyFileHandlingEvent;
    event BeforeApplyDirectoryHandling? BeforeApplyDirectoryHandlingEvent;
    event AfterApplyFileHandling? AfterApplyFileHandlingEvent;
    event AfterApplyDirectoryHandling? AfterApplyDirectoryHandlingEvent;

    DirectoryInfo TopLevelDirectory { get; }
    string GetTopLevelRelativePath(string path);
    string GetTopLevelRelativePath(FileSystemInfo file);
    FileHandlingParameters DefaultHandling { get; }

    IEnumerable<T> FinalItems<T>(bool canDeployOnly) where T : IFinalHandling;

    void Read();
}


