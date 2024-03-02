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
    public bool KeepEmptyDirectory { get; set; } = keepEmptyDirectory;
    public bool CheckedSuccessfully => true;
    
    IFinalDirectoryHandling IMutableDirectoryHandling.CheckAndTransform()
    {
        return this;
    }
}