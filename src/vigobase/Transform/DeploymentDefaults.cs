namespace vigobase;

public record DeploymentDefaults(
    string RepositoryPath,
    string DeploymentConfigFileName,
    UnixFileMode FileModeDefault,
    UnixFileMode DirectoryModeDefault,
    FileTypeEnum FileTypeDefault,
    FileEncodingEnum SourceFileEncodingDefault,
    FileEncodingEnum TargetFileEncodingDefault,
    LineEndingEnum LineEndingDefault,
    FilePermission FilePermissionDefault,
    bool TrailingNewlineDefault,
    string ValidCharactersDefault,
    IReadOnlyList<string> DefaultTargets)
{
    public string GetRepositoryRelativePath(string path)
    {
        return Path.GetRelativePath(RepositoryPath, path);
    }
    public string GetRepositoryRelativePath(FileInfo file)
    {
        return Path.GetRelativePath(RepositoryPath, file.FullName);
    }
}