using vigobase;

namespace vigo;

internal record AppConfigRepoDeploy(
    DirectoryInfo RepositoryRoot, 
    FileInfo OutputFile,
    IReadOnlyList<string> Targets
    ) : AppConfigRepo(RepositoryRoot)
{
    public override CommandEnum Command => CommandEnum.Deploy;
}