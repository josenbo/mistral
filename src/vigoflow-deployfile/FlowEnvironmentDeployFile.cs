using JetBrains.Annotations;
using vigobase;

namespace vigoflow_deployfile;

[PublicAPI]
public record FlowEnvironmentDeployFile(
    DirectoryInfo RepositoryRoot, 
    FileInfo Tarball
)
    : FlowEnvironment(RepositoryRoot, Tarball)
{
#pragma warning disable CA1822
    // Suppress the reminder to turn DeployFileName into a static member
    public string DeployfileName => "deployment.toml";
#pragma warning restore CA1822
}
