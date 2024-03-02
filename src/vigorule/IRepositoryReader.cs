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

    IEnumerable<IFinalHandling> FilesAndDirectories { get; }
    IEnumerable<IFinalFileHandling> Files { get; }
    IEnumerable<IFinalDirectoryHandling> Directories { get; }

    void Read();
}


