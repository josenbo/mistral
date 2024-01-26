namespace vigobase;

public static class DeploymentTransformationFactory
{
    public static IDeploymentTransformationReadWrite Create(FileInfo sourceFile, DeploymentDefaults defaults)
    {
        return new DeploymentTransformation(sourceFile, defaults);
    }
}