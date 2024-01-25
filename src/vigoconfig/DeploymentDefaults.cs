using vigobase;

namespace vigoconfig;

public record DeploymentDefaults(
    string DeploymentConfigFileName,
    UnixFileMode FileModeDefault,
    UnixFileMode DirectoryModeDefault,
    FileEncodingEnum SourceFileEncodingDefault,
    FileEncodingEnum TargetFileEncodingDefault,
    LineEndingEnum LineEndingDefault,
    bool TrailingNewlineDefault,
    FileTypeEnum FileTypeDefault
);