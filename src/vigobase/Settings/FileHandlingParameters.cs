namespace vigobase;

public record FileHandlingParameters(
    IAppSettings Settings,
    UnixFileMode StandardModeForFiles,
    UnixFileMode StandardModeForDirectories,
    FileTypeEnum FileType,
    FileEncodingEnum SourceFileEncoding,
    FileEncodingEnum TargetFileEncoding,
    LineEndingEnum LineEnding,
    FilePermission Permissions,
    bool FixTrailingNewline,
    string ValidChars,
    IReadOnlyList<string> Targets);
