namespace vigobase;

public interface IDeploymentTransformationReadOnlyDirectory : IDeploymentTransformationReadOnly
{
    DirectoryInfo SourceDirectory { get; }
    bool KeepEmptyDirectory { get; }
}