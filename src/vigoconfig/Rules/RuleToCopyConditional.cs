using vigobase;

namespace vigoconfig;

internal abstract record RuleToCopyConditional(
    int Index,
    string NameToMatch, 
    string NameReplacement,
    FileTypeEnum FileType,
    FileEncodingEnum SourceFileEncoding,
    FileEncodingEnum TargetFileEncoding,
    FilePermission FilePermission,
    LineEndingEnum LineEnding,
    bool FixTrailingNewline
) : RuleToCopy(
    Index,
    FileType,
    SourceFileEncoding,
    TargetFileEncoding,
    FilePermission,
    LineEnding,
    FixTrailingNewline
);