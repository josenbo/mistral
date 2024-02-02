using vigobase;

namespace vigoconfig;

internal class FolderConfigDataValidRule
{
    public RuleTypeEnum RuleType { get; init; } = RuleTypeEnum.Undefined;
    public string SourceFileName { get; init; } = string.Empty;
    public string TargetFileName { get; init; } = string.Empty;
    public string SourceFileNamePattern { get; init; } = string.Empty;
    public string TargetFileNamePattern { get; init; } = string.Empty;
    public FileTypeEnum FileType { get; set; }
    public FileEncodingEnum SourceFileEncoding { get; set; }
    public FileEncodingEnum TargetFileEncoding { get; set; }
    public LineEndingEnum LineEnding { get; set; }
    public FilePermission FilePermission { get; set; }
    public bool FixTrailingNewline { get; set; }
    public string ValidCharacters  { get; set; }
    public IList<string> Targets { get; }

    internal FolderConfigDataValidRule(DeploymentDefaults defaults)
    {
        FileType = defaults.FileTypeDefault;
        SourceFileEncoding = defaults.SourceFileEncodingDefault;
        TargetFileEncoding = defaults.TargetFileEncodingDefault;
        LineEnding = defaults.LineEndingDefault;
        FilePermission = defaults.FilePermissionDefault;
        FixTrailingNewline = defaults.TrailingNewlineDefault;
        ValidCharacters = defaults.ValidCharactersDefault;
        Targets = defaults.DefaultTargets.ToList();
    }
}
