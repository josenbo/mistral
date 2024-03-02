using vigobase;

namespace vigorule;

internal class DirectoryHandlingImpl
(
    DirectoryInfo sourceDirectory, 
    bool keepEmptyDirectory
)
: IMutableDirectoryHandling, IFinalDirectoryHandling
{
    public DirectoryInfo SourceDirectory { get; } = sourceDirectory;
    public bool KeepEmptyDirectory { get; } = keepEmptyDirectory;
    public bool IsEmptyDirectory { get; set; }
    public bool CanDeploy { get; set; }
    public bool CheckedSuccessfully => true;
    
    IFinalDirectoryHandling IMutableDirectoryHandling.CheckAndTransform()
    {
        return this;
    }
}