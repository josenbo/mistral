namespace vigobase;

public record FileTransformDescription(
    FileTypeEnum FileType,
    FileInfo SourceFile,
    FileEncodingEnum SourceFileEncoding,
    FileInfo TargetFile,
    FileEncodingEnum TargetFileEncoding,
    FilePermission FilePermission,
    LineEndingEnum LineEnding,
    bool FixTrailingNewline
);