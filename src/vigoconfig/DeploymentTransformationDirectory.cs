using vigobase;

namespace vigoconfig;

internal class DeploymentTransformationDirectory(DirectoryInfo sourceDirectory, bool keepEmptyDirectory)
    : IDeploymentTransformationReadWriteDirectory, IDeploymentTransformationReadOnlyDirectory
{
    public DirectoryInfo SourceDirectory { get; } = sourceDirectory;
    public bool KeepEmptyDirectory { get; set; } = keepEmptyDirectory;

    IDeploymentTransformationReadOnlyDirectory IDeploymentTransformationReadWriteDirectory.GetReadOnlyInterface()
    {
        return this;
    }
}