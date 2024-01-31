using vigobase;

namespace vigoconfig;

internal class FolderConfigDataValidRule
{
    public RuleTypeEnum RuleType { get; set; } = RuleTypeEnum.Undefined;
    public string SourceFileName { get; set; } = string.Empty;
    public string TargetFileName { get; set; } = string.Empty;
    public string SourceFileNamePattern { get; set; } = string.Empty;
    public string TargetFileNamePattern { get; set; } = string.Empty;
    public FileTypeEnum FileType { get; set; }
    public FileEncodingEnum SourceFileEncoding { get; set; }
    public FileEncodingEnum TargetFileEncoding { get; set; }
    public LineEndingEnum LineEnding { get; set; }
    public FilePermission FilePermission { get; set; }
    public bool FixTrailingNewline { get; set; }

    internal FolderConfigDataValidRule(DeploymentDefaults defaults)
    {
        FileType = defaults.FileTypeDefault;
        SourceFileEncoding = defaults.SourceFileEncodingDefault;
        TargetFileEncoding = defaults.TargetFileEncodingDefault;
        LineEnding = defaults.LineEndingDefault;
        FilePermission = FilePermission.UseDefault;
        FixTrailingNewline = defaults.TrailingNewlineDefault;
    }
}
