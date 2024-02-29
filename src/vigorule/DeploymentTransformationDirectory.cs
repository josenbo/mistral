using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigorule;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
internal class DeploymentTransformationDirectory(DirectoryInfo sourceDirectory, bool keepEmptyDirectory)
    : IDeploymentTransformationReadWriteDirectory, IDeploymentTransformationReadOnlyDirectory
{
    public DirectoryInfo SourceDirectory { get; } = sourceDirectory;
    public bool KeepEmptyDirectory { get; set; } = keepEmptyDirectory;
    public bool CheckedSuccessfully => true;
    
    IDeploymentTransformationReadOnlyDirectory IDeploymentTransformationReadWriteDirectory.CheckAndTransform()
    {
        return this;
    }
}