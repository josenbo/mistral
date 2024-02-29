using vigobase;

namespace vigo;

internal record AppConfigRepoCheck(
    DirectoryInfo RepositoryRoot
) : AppConfigRepo(RepositoryRoot)
{
    public override CommandEnum Command => CommandEnum.Check;
}