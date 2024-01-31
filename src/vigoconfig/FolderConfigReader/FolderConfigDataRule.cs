using vigobase;

namespace vigoconfig;

public class FolderConfigDataRule
{
    public string? RuleType { get; set; }
    public string? SourceFileName { get; set; }
    public string? TargetFileName { get; set; }
    public string? SourceFileNamePattern { get; set; }
    public string? TargetFileNamePattern { get; set; }
    public string? FileType { get; set; }
    public string? SourceFileEncoding { get; set; }
    public string? TargetFileEncoding { get; set; }
    public string? LineEnding { get; set; }
    public string? FilePermission { get; set; }
    public bool? FixTrailingNewline { get; set; }
    
    private bool IsCopyRule()
    {
        var ruleType = RuleType?.Trim().ToLower() ?? string.Empty;
        
        return ruleType switch
        {
            "copy" => true,
            "skip" => false,
            _ => throw new ArgumentException($"Invalid RuleType value {RuleType}")
        };
    }
    
    private static string CheckValidFileName(string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
    }
    
    private static string CheckValidFileNamePattern(string? pattern)
    {
        return string.IsNullOrWhiteSpace(pattern) ? string.Empty : pattern.Trim();
    }
    
    private static string CheckValidFileNamePatternReplacement(string? replacement)
    {
        return string.IsNullOrWhiteSpace(replacement) ? string.Empty : replacement.Trim();
    }

    internal FolderConfigDataValidRule GetValidRuleData(DeploymentDefaults defaults)
    {
        var isCopyRule = IsCopyRule();
        var sourceFileName = CheckValidFileName(SourceFileName);
        var targetFileName = CheckValidFileName(TargetFileName);
        var sourceFileNamePattern = CheckValidFileNamePattern(SourceFileNamePattern);
        var targetFileNamePattern = CheckValidFileNamePatternReplacement(TargetFileNamePattern);
        var ruleType = RuleTypeEnum.Undefined;
        
        if (isCopyRule)
        {
            if (!string.IsNullOrWhiteSpace(sourceFileName))
            {
                ruleType = RuleTypeEnum.CopyName;
                if (sourceFileName == targetFileName)
                    targetFileName = string.Empty;
                sourceFileNamePattern = string.Empty;
                targetFileNamePattern = string.Empty;
            }
            else if (!string.IsNullOrWhiteSpace(sourceFileNamePattern))
            {
                ruleType = RuleTypeEnum.CopyPattern;
                targetFileName = string.Empty;
            }
            else
            {
                ruleType = RuleTypeEnum.CopyAll;
                targetFileName = string.Empty;
                targetFileNamePattern = string.Empty;
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(sourceFileName))
            {
                ruleType = RuleTypeEnum.SkipName;
                if (sourceFileName == targetFileName)
                    targetFileName = string.Empty;
                sourceFileNamePattern = string.Empty;
                targetFileNamePattern = string.Empty;
            }
            else if (!string.IsNullOrWhiteSpace(sourceFileNamePattern))
            {
                ruleType = RuleTypeEnum.SkipPattern;
                targetFileName = string.Empty;
            }
            else
            {
                ruleType = RuleTypeEnum.SkipAll;
                targetFileName = string.Empty;
                targetFileNamePattern = string.Empty;
            }
        }
        
        var validRuleData = new FolderConfigDataValidRule(defaults)
        {
            RuleType = ruleType,
            SourceFileName = sourceFileName,
            TargetFileName = targetFileName,
            SourceFileNamePattern = sourceFileNamePattern,
            TargetFileNamePattern = targetFileNamePattern,
        };

        if (FileTypeEnumHelper.TryParse(FileType, out var fileType))
            validRuleData.FileType = fileType ?? throw new Exception("FileTypeEnumHelper.TryParse null constraint violated");
        
        if (FileEncodingEnumHelper.TryParse(SourceFileEncoding, out var sourceFileEncoding))
            validRuleData.SourceFileEncoding = sourceFileEncoding ?? throw new Exception("FileEncodingEnumHelper.TryParse null constraint violated");
        
        if (FileEncodingEnumHelper.TryParse(TargetFileEncoding, out var targetFileEncoding))
            validRuleData.TargetFileEncoding = targetFileEncoding ?? throw new Exception("FileEncodingEnumHelper.TryParse null constraint violated");
        
        if (LineEndingEnumHelper.TryParse(LineEnding, out var lineEnding))
            validRuleData.LineEnding = lineEnding ?? throw new Exception("LineEndingEnumHelper.TryParse null constraint violated");

        if (vigobase.FilePermission.TryParse(FilePermission, out var filePermission))
            validRuleData.FilePermission = filePermission;

        if (FixTrailingNewline.HasValue)
            validRuleData.FixTrailingNewline = FixTrailingNewline.Value;
        
        return validRuleData;
    }
}