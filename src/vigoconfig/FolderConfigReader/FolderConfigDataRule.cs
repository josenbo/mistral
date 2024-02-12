using System.Runtime.Serialization;
using Serilog;
using vigobase;

namespace vigoconfig;

public class FolderConfigDataRule
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
    
    internal FileRule GetFileRule(IFolderConfiguration folderConfig, FileHandlingParameters handlingDefaults)
    {
        var relativePath = handlingDefaults.Settings.GetRepoRelativePath(folderConfig.Location);
        
        var handling = handlingDefaults;

        if (FileType is not null && FileTypeEnumHelper.TryParse(FileType, out var fileType))
            handling = handling with { FileType = fileType.Value };
        
        if (SourceFileEncoding is not null && FileEncodingEnumHelper.TryParse(SourceFileEncoding, out var sourceFileEncoding))
            handling = handling with { SourceFileEncoding = sourceFileEncoding.Value };
        
        if (TargetFileEncoding is not null && FileEncodingEnumHelper.TryParse(TargetFileEncoding, out var targetFileEncoding))
            handling = handling with { TargetFileEncoding = targetFileEncoding.Value };

        if (LineEnding is not null && LineEndingEnumHelper.TryParse(LineEnding, out var lineEnding))
            handling = handling with { LineEnding = lineEnding.Value };
        
        if (FilePermission is not null && vigobase.FilePermission.TryParse(FilePermission, out var filePermission))
            handling = handling with { Permissions = filePermission } ;

        if (FixTrailingNewline.HasValue)
            handling = handling with { FixTrailingNewline = FixTrailingNewline.Value };

        if (ValidCharacters is not null)
            handling = handling with { ValidCharsRegex = ValidCharactersHelper.ParseConfiguration(ValidCharacters) }; 
        
        if (Targets != null)
            handling = handling with { Targets = DeploymentTargetHelper.ParseTargets(Targets).ToList() };
        
        var action = GetAction();
        
        var actionName = action switch
        {
            FileRuleActionEnum.SkipRule => "Skip",
            FileRuleActionEnum.CopyRule => "Copy",
            FileRuleActionEnum.CheckRule => "Check",
            FileRuleActionEnum.Undefined => throw new ArgumentException($"Invalid action enum \"{action}\""),
            _ => throw new ArgumentException($"Unknown action enum \"{action}\"")
        };

        var (condition, lookFor, replaceWith) = GetCondition(relativePath);

        var ruleIndex = folderConfig.NextRuleIndex;
        
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return condition switch
        {
            FileRuleConditionEnum.Unconditional => new FileRuleUnconditional(
                Id: new FileRuleId(relativePath, ruleIndex, $"#{ruleIndex} {actionName} unconditionally"),
                Action: action,
                Handling: handling
                ),
            FileRuleConditionEnum.MatchName => new FileRuleMatchingLiteral(
                Id: new FileRuleId(relativePath, ruleIndex, $"#{ruleIndex} {actionName} when name equals '{lookFor}"),
                Action: action,
                NameToMatch: lookFor,
                NameReplacement: replaceWith,
                Handling: handling
                ),
            FileRuleConditionEnum.MatchPattern => new FileRuleMatchingPattern(
                Id: new FileRuleId(relativePath, ruleIndex, $"#{ruleIndex} {actionName} when name matches '{lookFor}"),
                Action: action,
                NameToMatch: lookFor,
                NameReplacement: replaceWith,
                Handling: handling
                ),
            _ => throw new ArgumentOutOfRangeException($"The condition {condition} is not allowed or not understood in {relativePath}", nameof(condition))
        };
    }

    private FileRuleActionEnum GetAction()
    {
        var ruleType = RuleType?.Trim().ToLower() ?? string.Empty;
        
        return ruleType switch
        {
            "copy" => FileRuleActionEnum.CopyRule,
            "check" => FileRuleActionEnum.CheckRule,
            "skip" => FileRuleActionEnum.SkipRule,
            _ => throw new ArgumentException($"Invalid or missing action (Value: {RuleType})", nameof(ruleType))
        };
    }

    private (FileRuleConditionEnum condition, string lookFor, string replaceWith) GetCondition(string relativePath)
    {
        var sourceFileName = CheckValidFileName(SourceFileName);
        var targetFileName = CheckValidFileName(TargetFileName);
        var sourceFileNamePattern = CheckValidFileNamePattern(SourceFileNamePattern);
        var targetFileNamePattern = CheckValidFileNamePatternReplacement(TargetFileNamePattern);

        if (!string.IsNullOrWhiteSpace(sourceFileName))
        {
            if (sourceFileName == targetFileName)
                targetFileName = string.Empty;
            
            // ReSharper disable once InvertIf
            if (!string.IsNullOrEmpty(sourceFileNamePattern) || !string.IsNullOrEmpty(targetFileNamePattern))
            {
                Log.Warning("The condition \"{TheRuleType} when name is {SourceFileName}\" in the directory {TheDirectory} has a redundant file name pattern and/or replacement, which will be ignored",
                    RuleType,
                    SourceFileName,
                    relativePath
                    );
                sourceFileNamePattern = string.Empty;
                targetFileNamePattern = string.Empty;
            }

            return (FileRuleConditionEnum.MatchName, sourceFileName, targetFileName);
        }
        else if (!string.IsNullOrWhiteSpace(sourceFileNamePattern))
        {
            targetFileName = string.Empty;
            
            // ReSharper disable once InvertIf
            if (!string.IsNullOrEmpty(targetFileName))
            {
                Log.Warning("The condition \"{TheRuleType} when name matches {SourceFileNamePattern}\" in the directory {TheDirectory} has a redundant literal file name replacement, which will be ignored",
                    RuleType,
                    SourceFileNamePattern,
                    relativePath
                );
                targetFileName = string.Empty;
            }

            return (FileRuleConditionEnum.MatchPattern, sourceFileNamePattern, targetFileNamePattern);
        }
        else
        {
            // ReSharper disable once InvertIf
            if (!string.IsNullOrEmpty(targetFileName) || !string.IsNullOrEmpty(targetFileNamePattern))
            {
                Log.Warning("The condition \"{TheRuleType} unconditionally\" in the directory {TheDirectory} has redundant file name replacements, which will be ignored",
                    RuleType,
                    relativePath
                );
                targetFileName = string.Empty;
                targetFileNamePattern = string.Empty;
            }

            return (FileRuleConditionEnum.Unconditional, string.Empty, string.Empty);
        }
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
}