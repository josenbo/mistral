using vigobase;

namespace vigoflow_deployfile;

public static class DeployfileFlowFactory
{
    public static IFlow Create(DirectoryInfo repositoryRoot, FileInfo tarball)
    {
        return new DeployfileFlow();
    }
}