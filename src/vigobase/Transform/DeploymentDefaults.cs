namespace vigobase;

public record DeploymentDefaults(
    string RepositoryPath,
    string DeploymentConfigFileName,
    UnixFileMode FileModeDefault,
    UnixFileMode DirectoryModeDefault,
    FileEncodingEnum SourceFileEncodingDefault,
    FileEncodingEnum TargetFileEncodingDefault,
    LineEndingEnum LineEndingDefault,
    bool TrailingNewlineDefault,
    FileTypeEnum FileTypeDefault
)
{
    public string GetRepositoryRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(RepositoryPath) || string.IsNullOrWhiteSpace(path))
            return path;

        if (!path.StartsWith(RepositoryPath))
            return path;

        return RepositoryPath.Length < path.Length ? path[RepositoryPath.Length..] : string.Empty;
    }
}