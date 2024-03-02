using System.Text;
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

    public void Explain(StringBuilder sb, ExplainSettings settings)
    {
        // todo: add implementation
        sb.Append(nameof(DirectoryHandlingImpl))
            .Append('.')
            .Append(nameof(Explain))
            .AppendLine(" still needs to be implemented");
    }
    
    IFinalDirectoryHandling IMutableDirectoryHandling.CheckAndTransform()
    {
        return this;
    }
}