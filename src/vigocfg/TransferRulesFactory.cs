using System.Text.RegularExpressions;
using Ardalis.GuardClauses;

namespace vigocfg;

internal static class TransferRulesFactory
{
    internal static IEnumerable<TransferRule> GetRules(string username, string foldername)
    {
        // ReSharper disable once InvertIf
        if (!_isInitialized)
        {
            var crontabRules = GetTransferRulesForCrontab();
            DictRules.Add(("cupines", "crontab"), crontabRules);
            DictRules.Add(("stammaus", "crontab"), crontabRules);
            DictRules.Add(("cupines", "local-bin"), GetTransferRulesForCupinesLocalBin());
            DictRules.Add(("stammaus", "local-bin"), GetTransferRulesForStammausLocalBin());
            DictRules.Add(("cupines", "local-data"), GetTransferRulesForCupinesLocalData());
            DictRules.Add(("stammaus", "local-data"), GetTransferRulesForStammausLocalData());

            foreach (var rulesList in DictRules.Values)
            {
                AppendSkipAll(rulesList);
            }
            
            _isInitialized = true;
        }
            
        if (DictRules.TryGetValue((username, foldername), out var listOfRules))
            return listOfRules;

        listOfRules = new List<TransferRule>();
        AppendSkipAll(listOfRules);
        return listOfRules;
    }

    #region Populate Rules

    private static List<TransferRule> GetTransferRulesForCupinesLocalBin()
    {
        var rules = new List<TransferRule>();

        var sourceDefaults = new SourceFileProperties(
            FileTypeEnum.TextFile, 
            FileEncodingEnum.UTF_8);
        
        var targetDefaults = new TargetFileProperties(
            FileEncodingEnum.ISO_8859_1, 
            EnvironmentHelper.DefaultLineEnding, 
            true, 
            FilePermissionValue.FromSymbolicNotation("u+x"));

        var targetAscii = targetDefaults with { FileEncoding = FileEncodingEnum.Ascii };
        
        AppendMatchName(rules, "cluster.cron", string.Empty, sourceDefaults, targetAscii);
        AppendMatchName(rules, "export.config", string.Empty, sourceDefaults, targetAscii);
        AppendMatchName(rules, "import.config", string.Empty, sourceDefaults, targetAscii);
        AppendMatchName(rules, "jbossrestart.sh", string.Empty, sourceDefaults, targetAscii);
        
        var targetUtf8 = targetDefaults with { FileEncoding = FileEncodingEnum.UTF_8 };
        
        AppendMatchName(rules, "pb-export", string.Empty, sourceDefaults, targetUtf8);
        AppendMatchName(rules, "pb-import", string.Empty, sourceDefaults, targetUtf8);
        
        AppendMatchAll(rules, sourceDefaults, targetDefaults);
        
        return rules;
    }

    private static List<TransferRule> GetTransferRulesForCupinesLocalData()
    {
        var rules = new List<TransferRule>();

        var sourceDefaults = new SourceFileProperties(
            FileTypeEnum.TextFile, 
            FileEncodingEnum.UTF_8);
        
        var targetDefaults = new TargetFileProperties(
            FileEncodingEnum.ISO_8859_1, 
            EnvironmentHelper.DefaultLineEnding, 
            true, 
            UndefinedFilePermission);

        var targetAscii = targetDefaults with { FileEncoding = FileEncodingEnum.Ascii };
        
        AppendMatchName(rules, "email-1.html", string.Empty, sourceDefaults, targetAscii);
        AppendMatchName(rules, "email.csv", string.Empty, sourceDefaults, targetAscii);
        AppendMatchName(rules, "email-eingangmithorn.html", string.Empty, sourceDefaults, targetAscii);
        AppendMatchName(rules, "email-header.txt", string.Empty, sourceDefaults, targetAscii);
        AppendMatchName(rules, "email.txt", string.Empty, sourceDefaults, targetAscii);
        AppendMatchName(rules, "load_impexp.ctl", string.Empty, sourceDefaults, targetAscii);
        
        AppendMatchAll(rules, sourceDefaults, targetDefaults);

        return rules;
    }

    private static List<TransferRule> GetTransferRulesForStammausLocalBin()
    {
        var rules = new List<TransferRule>();

        var sourceDefaults = new SourceFileProperties(
            FileTypeEnum.TextFile, 
            FileEncodingEnum.UTF_8);
        
        var targetDefaults = new TargetFileProperties(
            FileEncodingEnum.ISO_8859_1, 
            EnvironmentHelper.DefaultLineEnding, 
            true, 
            FilePermissionValue.FromSymbolicNotation("u+x"));

        var targetAscii = targetDefaults with { FileEncoding = FileEncodingEnum.Ascii };
        
        AppendMatchName(rules, "cluster.cron", string.Empty, sourceDefaults, targetAscii);
        AppendMatchName(rules, "export.config", string.Empty, sourceDefaults, targetAscii);
        AppendMatchName(rules, "import.config", string.Empty, sourceDefaults, targetAscii);
        AppendMatchName(rules, "log-reload", string.Empty, sourceDefaults, targetAscii);
        
        var targetUtf8 = targetDefaults with { FileEncoding = FileEncodingEnum.UTF_8 };
        
        AppendMatchName(rules, "pba-ems", string.Empty, sourceDefaults, targetUtf8);
        AppendMatchName(rules, "pb-cds", string.Empty, sourceDefaults, targetUtf8);
        AppendMatchName(rules, "pb-export", string.Empty, sourceDefaults, targetUtf8);
        AppendMatchName(rules, "pb-import", string.Empty, sourceDefaults, targetUtf8);
        
        AppendMatchPattern(rules, @"^load\.[-a-z_0-9]+$", string.Empty, sourceDefaults, targetUtf8);
        
        AppendMatchAll(rules, sourceDefaults, targetDefaults);

        return rules;
    }

    private static List<TransferRule> GetTransferRulesForStammausLocalData()
    {
        var rules = new List<TransferRule>();

        var sourceDefaults = new SourceFileProperties(
            FileTypeEnum.TextFile, 
            FileEncodingEnum.UTF_8);
        
        var targetDefaults = new TargetFileProperties(
            FileEncodingEnum.Ascii, 
            EnvironmentHelper.DefaultLineEnding, 
            true, 
            UndefinedFilePermission);

        var targetIso8859X1 = targetDefaults with { FileEncoding = FileEncodingEnum.ISO_8859_1 };
        
        AppendMatchName(rules, "actions.dat", string.Empty, sourceDefaults, targetIso8859X1);
        AppendMatchName(rules, "cupevents.dat", string.Empty, sourceDefaults, targetIso8859X1);
        AppendMatchName(rules, "isocodes.dat", string.Empty, sourceDefaults, targetIso8859X1);
        AppendMatchName(rules, "kassen-cup.dat", string.Empty, sourceDefaults, targetIso8859X1);
        AppendMatchName(rules, "kassen-int.dat", string.Empty, sourceDefaults, targetIso8859X1);
        AppendMatchName(rules, "pba_parameter.dat", string.Empty, sourceDefaults, targetIso8859X1);
        AppendMatchName(rules, "produktart.dat", string.Empty, sourceDefaults, targetIso8859X1);
        AppendMatchName(rules, "reasons.dat", string.Empty, sourceDefaults, targetIso8859X1);
        AppendMatchName(rules, "rechner2ort.dat", string.Empty, sourceDefaults, targetIso8859X1);
        AppendMatchName(rules, "tbzlevents.dat", string.Empty, sourceDefaults, targetIso8859X1);
        AppendMatchName(rules, "upu.dat", string.Empty, sourceDefaults, targetIso8859X1);
        AppendMatchName(rules, "zustellcodes.dat", string.Empty, sourceDefaults, targetIso8859X1);
        
        AppendMatchAll(rules, sourceDefaults, targetDefaults);

        return rules;
    }
    
    private static List<TransferRule> GetTransferRulesForCrontab()
    {
        var rules = new List<TransferRule>();

        var sourceDefaults = new SourceFileProperties(FileTypeEnum.TextFile, FileEncodingEnum.UTF_8);
        var targetDefaults = new TargetFileProperties(FileEncodingEnum.UTF_8, EnvironmentHelper.DefaultLineEnding, true, UndefinedFilePermission);

        AppendMatchName(rules, "crontab", string.Empty, sourceDefaults, targetDefaults);
        AppendSkipAll(rules);
        
        return rules;
    }

    #endregion    

    #region Helper
    
    private static void AppendMatchName(
        ICollection<TransferRule> rules,
        string nameToMatch,
        string nameReplacementOrEmptyToKeep,
        SourceFileProperties sourceFileProperties,
        TargetFileProperties targetFileProperties
    )
    {
        AppendRule(
            rules,
            nameToMatch,
            nameReplacementOrEmptyToKeep,
            false,
            false,
            true,
            sourceFileProperties.FileType,
            sourceFileProperties.FileEncoding,
            targetFileProperties.FileEncoding,
            targetFileProperties.LineEnding,
            targetFileProperties.AppendFinalNewline,
            targetFileProperties.FilePermission);
    }

    private static void AppendMatchPattern(
        ICollection<TransferRule> rules,
        string patternToMatch,
        string patternReplacementOrEmptyToKeep,
        SourceFileProperties sourceFileProperties,
        TargetFileProperties targetFileProperties
    )
    {
        AppendRule(
            rules,
            patternToMatch,
            patternReplacementOrEmptyToKeep,
            false,
            true,
            true,
            sourceFileProperties.FileType,
            sourceFileProperties.FileEncoding,
            targetFileProperties.FileEncoding,
            targetFileProperties.LineEnding,
            targetFileProperties.AppendFinalNewline,
            targetFileProperties.FilePermission);
    }
    
    private static void AppendMatchAll(
        ICollection<TransferRule> rules,
        SourceFileProperties sourceFileProperties,
        TargetFileProperties targetFileProperties
    )
    {
        AppendRule(
            rules,
            "^.*$",
            string.Empty,
            true,
            true,
            true,
            sourceFileProperties.FileType,
            sourceFileProperties.FileEncoding,
            targetFileProperties.FileEncoding,
            targetFileProperties.LineEnding,
            targetFileProperties.AppendFinalNewline,
            targetFileProperties.FilePermission);
    }

    // ReSharper disable once UnusedMember.Local
    private static void AppendSkipName(ICollection<TransferRule> rules, string nameToMatch)
    {
        AppendRule(
            rules,
            nameToMatch,
            string.Empty,
            false,
            false,
            false,
            FileTypeEnum.Undefined,
            FileEncodingEnum.Undefined,
            FileEncodingEnum.Undefined,
            LineEndingEnum.Undefined,
            false,
            UndefinedFilePermission);
    }

    // ReSharper disable once UnusedMember.Local
    private static void AppendSkipPattern(ICollection<TransferRule> rules, string patternToMatch)
    {
        AppendRule(
            rules,
            patternToMatch,
            string.Empty,
            false,
            true,
            false,
            FileTypeEnum.Undefined,
            FileEncodingEnum.Undefined,
            FileEncodingEnum.Undefined,
            LineEndingEnum.Undefined,
            false,
            UndefinedFilePermission);
    }

    private static void AppendSkipAll(ICollection<TransferRule> rules)
    {
        AppendRule(
            rules,
            "^.*$",
            string.Empty,
            true,
            true,
            false,
            FileTypeEnum.Undefined,
            FileEncodingEnum.Undefined,
            FileEncodingEnum.Undefined,
            LineEndingEnum.Undefined,
            false,
            UndefinedFilePermission);
    }
    
    private static void AppendRule(
        ICollection<TransferRule> rules, 
        string nameToMatch, 
        string nameReplacement, 
        bool isCatchAll, 
        bool isPattern, 
        bool isAllowRule,
        FileTypeEnum sourceFileType,
        FileEncodingEnum sourceEncoding, 
        FileEncodingEnum targetEncoding, 
        LineEndingEnum targetLineEnding, 
        bool appendFinalNewline, 
        FilePermissionValue targetFilePermission)
    {
        nameToMatch = Guard.Against.NullOrWhiteSpace(nameToMatch, nameof(nameToMatch)).Trim();
        nameReplacement = isAllowRule ? nameReplacement.Trim() : string.Empty;
        Guard.Against.InvalidInput(isCatchAll, nameof(isCatchAll), c => !c || (c && isPattern));
        Guard.Against.InvalidInput(isAllowRule, nameof(isAllowRule), c => c || (!c && string.IsNullOrEmpty(nameReplacement)));

        if (isAllowRule)
        {
            Guard.Against.InvalidInput(sourceFileType, nameof(sourceFileType), t => t is FileTypeEnum.BinaryFile or FileTypeEnum.TextFile);

            if (sourceFileType == FileTypeEnum.TextFile)
            {
                Guard.Against.InvalidInput(sourceEncoding, nameof(sourceEncoding),
                    e => e is 
                        FileEncodingEnum.UTF_8 or 
                        FileEncodingEnum.ISO_8859_15 or 
                        FileEncodingEnum.ISO_8859_1 or 
                        FileEncodingEnum.Ascii or 
                        FileEncodingEnum.Windows_1252);

                Guard.Against.InvalidInput(targetEncoding, nameof(targetEncoding),
                    e => e is 
                        FileEncodingEnum.UTF_8 or 
                        FileEncodingEnum.ISO_8859_15 or 
                        FileEncodingEnum.ISO_8859_1 or 
                        FileEncodingEnum.Ascii or 
                        FileEncodingEnum.Windows_1252);

                Guard.Against.InvalidInput(targetLineEnding, nameof(targetLineEnding),
                    l => l is LineEndingEnum.LF or LineEndingEnum.CR_LF);
            }
            else
            {
                sourceEncoding = FileEncodingEnum.Undefined;
                targetEncoding = FileEncodingEnum.Undefined;
                targetLineEnding = LineEndingEnum.Undefined;
                appendFinalNewline = false;
            }
        }
        else
        {
            sourceFileType = FileTypeEnum.Undefined;
            sourceEncoding = FileEncodingEnum.Undefined;
            targetFilePermission = UndefinedFilePermission;
            targetEncoding = FileEncodingEnum.Undefined;
            targetLineEnding = LineEndingEnum.Undefined;
            appendFinalNewline = false;
        }
        
        var rule = new TransferRule(
            nameToMatch, 
            nameReplacement, 
            isCatchAll, 
            isPattern,
            isPattern ? new Regex(nameToMatch, RegexOptions.Compiled) : null,
            !isAllowRule, 
            isAllowRule,
            sourceFileType, 
            sourceEncoding, 
            targetFilePermission,
            targetEncoding, 
            targetLineEnding,
            appendFinalNewline);
        
        rules.Add(rule);
    }

    #endregion
    
    private static bool _isInitialized;
    private static readonly FilePermissionValueUndefined UndefinedFilePermission = FilePermissionValue.Undefined;
    private static readonly Dictionary<(string username, string foldername), List<TransferRule>> DictRules = new();
}