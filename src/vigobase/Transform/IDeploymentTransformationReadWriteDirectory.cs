namespace vigobase;

public interface IDeploymentTransformationReadWriteDirectory : IDeploymentTransformationReadWrite
{
    DirectoryInfo SourceDirectory { get; }
    bool KeepEmptyDirectory { get; }
    IDeploymentTransformationReadOnlyDirectory CheckAndTransform();
}