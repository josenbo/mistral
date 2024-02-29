
using vigobase;

namespace vigo;

internal abstract record AppConfigRepo(
    DirectoryInfo RepositoryRoot
) : AppConfig
{
    public static string OutputFileTempPath => Path.Combine(AppEnv.TemporaryDirectory.FullName, "vigo.tar.gz");
}