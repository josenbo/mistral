using vigobase;

namespace vigoconfig;

internal abstract record RuleToCopy(
    int Index,
    FileTypeEnum FileType,
    FileEncodingEnum SourceFileEncoding,
    FileEncodingEnum TargetFileEncoding,
    FilePermission FilePermission,
    LineEndingEnum LineEnding,
    bool FixTrailingNewline
    ) : Rule(Index);