using System.Text.RegularExpressions;

namespace vigocfg;

public interface ITransferRule
{
    string NameToMatch { get; }
    string NameReplacement { get; }
    bool IsCatchAll { get; }
    bool IsPattern { get; }
    Regex? RexPattern { get; }
    bool IsDenyRule { get; }
    bool IsAllowRule { get; }
    FileTypeEnum SourceFileType { get; }
    FileEncodingEnum SourceEncoding { get; }
    FilePermission TargetFilePermission { get; }
    FileEncodingEnum TargetEncoding { get; }
    LineEndingEnum TargetLineEnding { get; }
    bool AppendFinalNewline { get; }
}