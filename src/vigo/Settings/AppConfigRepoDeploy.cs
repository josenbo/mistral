using vigobase;

namespace vigo;

internal record AppConfigRepoDeploy(
    DirectoryInfo RepositoryRoot, 
    FileInfo Tarball
) : AppConfigRepo(RepositoryRoot)
{
    public override CommandEnum Command => CommandEnum.Deploy;
}