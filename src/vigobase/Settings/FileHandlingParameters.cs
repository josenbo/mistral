namespace vigobase;

public record FileHandlingParameters(
    IAppSettings AppSettings,
    UnixFileMode FileModeDefault,
    UnixFileMode DirectoryModeDefault,
    FileTypeEnum FileTypeDefault,
    FileEncodingEnum SourceFileEncodingDefault,
    FileEncodingEnum TargetFileEncodingDefault,
    LineEndingEnum LineEndingDefault,
    FilePermission FilePermissionDefault,
    bool TrailingNewlineDefault,
    string ValidCharactersDefault,
    IReadOnlyList<string> DefaultTargets);
