using System.Text.RegularExpressions;

namespace vigocfg;

internal record TransferRule(
    string NameToMatch,
    string NameReplacement,
    bool IsCatchAll,
    bool IsPattern,
    Regex? RexPattern,
    bool IsDenyRule,
    bool IsAllowRule,
    FileTypeEnum SourceFileType,
    FileEncodingEnum SourceEncoding,
    FilePermission TargetFilePermission,
    FileEncodingEnum TargetEncoding,
    LineEndingEnum TargetLineEnding,
    bool AppendFinalNewline
) : ITransferRule;