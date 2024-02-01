using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Serilog;
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
    public string? Buckets { get; set; }
    
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

    private static IEnumerable<string> CheckValidBuckets(string? buckets)
    {
        if (string.IsNullOrWhiteSpace(buckets))
            yield break;

        foreach (var bucket in  buckets.Split(
                         BucketSeparators,
                         StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                         ).Distinct(StringComparer.InvariantCultureIgnoreCase))
        {
            if (!RexBucket.IsMatch(bucket))
            {
                Log.Fatal("Invalid bucket name {TheBucketName}", bucket);
                throw new VigoFatalException("Invalid bucket name");
            }

            yield return bucket;
        }
    }

    internal FolderConfigDataValidRule GetValidRuleData(DeploymentDefaults defaults)
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

        foreach (var bucket in CheckValidBuckets(Buckets))
        {
            validRuleData.Buckets.Add(bucket);
        }
        
        return validRuleData;
    }
    
    private static readonly char[] BucketSeparators = new char[] {' ', ',', ';'};
    private static readonly Regex RexBucket = CompiledRexBucket();

    [GeneratedRegex("^[a-zA-Z]([-_.]?[a-zA-Z0-9]{1,40}){1,40}$")]
    private static partial Regex CompiledRexBucket();
}