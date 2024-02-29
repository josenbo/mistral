namespace vigobase;

public interface IRepositoryReader
{
    delegate void BeforeApplyFileTransformation(IDeploymentTransformationReadWriteFile transformation);
    delegate void BeforeApplyDirectoryTransformation(IDeploymentTransformationReadWriteDirectory transformation);

    event BeforeApplyFileTransformation? BeforeApplyFileTransformationEvent;
    event BeforeApplyDirectoryTransformation? BeforeApplyDirectoryTransformationEvent;
    IEnumerable<IDeploymentTransformationReadOnlyFile> FileTransformations { get; }
    IEnumerable<IDeploymentTransformationReadOnlyDirectory> DirectoryTransformations { get; }
}


