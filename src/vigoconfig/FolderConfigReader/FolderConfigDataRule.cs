using System.Runtime.Serialization;
using vigobase;

namespace vigoconfig;

public partial class FolderConfigDataRule
{
    [DataMember(Name = "Action")]   
    public string? RuleType { get; set; }

    [DataMember(Name = "WhenNameIsEqual")]   
    public string? SourceFileName { get; set; }

    [DataMember(Name = "ChangeName")]   
    public string? TargetFileName { get; set; }

    [DataMember(Name = "WhenNameMatchesPattern")]   
    public string? SourceFileNamePattern { get; set; }

    [DataMember(Name = "TransformNameByRule")]   
    public string? TargetFileNamePattern { get; set; }

    [DataMember(Name = "ValidChars")]   
    public string? ValidCharacters { get; set; }

    [DataMember(Name = "FileType")]   
    public string? FileType { get; set; }

    [DataMember(Name = "StoredWithEncoding")]   
    public string? SourceFileEncoding { get; set; }

    [DataMember(Name = "DeployWithEncoding")]   
    public string? TargetFileEncoding { get; set; }

    [DataMember(Name = "Newline")]   
    public string? LineEnding { get; set; }

    [DataMember(Name = "FileMode")]   
    public string? FilePermission { get; set; }

    [DataMember(Name = "FixTrailingNewline")]   
    public bool? FixTrailingNewline { get; set; }
    
    [DataMember(Name = "TargetList")]   
    public string? Targets { get; set; }
    
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

    internal FolderConfigDataValidRule GetValidRuleData(FileHandlingParameters defaults)
    {
        var isCopyRule = IsCopyRule();
        var sourceFileName = CheckValidFileName(SourceFileName);
        var targetFileName = CheckValidFileName(TargetFileName);
        var sourceFileNamePattern = CheckValidFileNamePattern(SourceFileNamePattern);
        var targetFileNamePattern = CheckValidFileNamePatternReplacement(TargetFileNamePattern);
        RuleTypeEnum ruleType;
        
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

        if (FileType is not null && FileTypeEnumHelper.TryParse(FileType, out var fileType))
            validRuleData.FileType = fileType ?? throw new Exception("FileTypeEnumHelper.TryParse null constraint violated");
        
        if (SourceFileEncoding is not null && FileEncodingEnumHelper.TryParse(SourceFileEncoding, out var sourceFileEncoding))
            validRuleData.SourceFileEncoding = sourceFileEncoding ?? throw new Exception("FileEncodingEnumHelper.TryParse null constraint violated");
        
        if (TargetFileEncoding is not null && FileEncodingEnumHelper.TryParse(TargetFileEncoding, out var targetFileEncoding))
            validRuleData.TargetFileEncoding = targetFileEncoding ?? throw new Exception("FileEncodingEnumHelper.TryParse null constraint violated");
        
        if (LineEnding is not null && LineEndingEnumHelper.TryParse(LineEnding, out var lineEnding))
            validRuleData.LineEnding = lineEnding ?? throw new Exception("LineEndingEnumHelper.TryParse null constraint violated");
        
        if (FilePermission is not null && vigobase.FilePermission.TryParse(FilePermission, out var filePermission))
            validRuleData.FilePermission = filePermission;

        if (FixTrailingNewline.HasValue)
            validRuleData.FixTrailingNewline = FixTrailingNewline.Value;

        if (ValidCharacters is not null)
            validRuleData.ValidCharacters = ValidCharactersHelper.ParseConfiguration(ValidCharacters); 
        
        // ReSharper disable once InvertIf
        if (Targets != null)
        {
            // delete the inherited defaults
            validRuleData.Targets.Clear();
            
            foreach (var target in DeploymentTargetHelper.ParseTargets(Targets))
            {
                validRuleData.Targets.Add(target);
            }
        }
        
        return validRuleData;
    }
}