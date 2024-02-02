using System.Diagnostics.CodeAnalysis;
using System.Text;
using Serilog;
using Tomlyn;
using Tomlyn.Syntax;
using vigobase;

namespace vigoconfig;

internal static class FolderConfigurator
{
    public static void Configure(IFolderConfiguration config)
    {
        try
        {
            if (!TryReadFileContent(config.Location, config.GlobalDefaults.DeploymentConfigFileName, out var content))
            {
                config.AddRule(BuildDefaultRuleSkipAll(config.NextRuleIndex));
                return;
            }
            
            Log.Information("Found a deployment configuration file in the directory {TheDirectory}",
                config.GlobalDefaults.GetRepositoryRelativePath(config.Location.FullName));
        
            if (!TryParseConfiguration(content, out var tomlConfigurationData))
            {
                Log.Fatal("Could not read the configuration file {TheFileName}}",
                    config.GlobalDefaults.DeploymentConfigFileName);

                throw new VigoFatalException("Could not read the folder configuration");
            }

            config.HasKeepFolderFlag = tomlConfigurationData.KeepEmptyFolder is true;
            config.SetLocalDefaults(tomlConfigurationData.GetLocalDefaults(config.GlobalDefaults));
                
            ValidateAndAppendRules(config, tomlConfigurationData.Rules);

            config.AddRule(BuildDefaultRuleCopyAll(config.GlobalDefaults, config.NextRuleIndex));
        }
        catch (Exception e) when (e is not VigoException)
        {
            Log.Fatal(e,"Could not read the configuration in the directory {TheDir}",
                config.GlobalDefaults.GetRepositoryRelativePath(config.Location.FullName));
            throw new VigoFatalException("Failed to read the deployment configuration in the repository folder");
        }
    }

    private static void ValidateAndAppendRules(IFolderConfiguration config, IEnumerable<FolderConfigDataRule> tomlRuleData)
    {
        foreach (var data in tomlRuleData.Select(d => d.GetValidRuleData(config.LocalDefaults)))
        {
            switch (data.RuleType)
            {
                case RuleTypeEnum.CopyAll:
                    config.AddRule(new RuleToCopyUnconditional(
                        config.NextRuleIndex, 
                        data.FileType,
                        data.SourceFileEncoding,
                        data.TargetFileEncoding,
                        data.FilePermission,
                        data.LineEnding,
                        data.FixTrailingNewline));
                    break;
                case RuleTypeEnum.CopyName:
                    config.AddRule(new RuleToCopyMatchingLiteral(
                        config.NextRuleIndex,
                        data.SourceFileName,
                        data.TargetFileName,
                        data.FileType,
                        data.SourceFileEncoding,
                        data.TargetFileEncoding,
                        data.FilePermission,
                        data.LineEnding,
                        data.FixTrailingNewline));
                    break;
                case RuleTypeEnum.CopyPattern:
                    config.AddRule(new RuleToCopyMatchingPattern(
                        config.NextRuleIndex,
                        data.SourceFileNamePattern,
                        data.TargetFileNamePattern,
                        data.FileType,
                        data.SourceFileEncoding,
                        data.TargetFileEncoding,
                        data.FilePermission,
                        data.LineEnding,
                        data.FixTrailingNewline));
                    break;
                case RuleTypeEnum.SkipAll:
                    config.AddRule(new RuleToSkipUnconditional(config.NextRuleIndex));
                    break;
                case RuleTypeEnum.SkipName:
                    config.AddRule(new RuleToSkipMatchingLiteral(config.NextRuleIndex, data.SourceFileName));
                    break;
                case RuleTypeEnum.SkipPattern:
                    config.AddRule(new RuleToSkipMatchingPattern(config.NextRuleIndex, data.SourceFileNamePattern));
                    break;
                case RuleTypeEnum.Undefined:
                default:
                    throw new ArgumentOutOfRangeException($"Invalid rule type {data.RuleType}");
            }
        }    
    }

    private static bool TryReadFileContent(DirectoryInfo? directory, string deploymentConfigFileName, [NotNullWhen(true)] out string? content)
    {
        if (directory is null)
        {
            content = null;
            return false;
        }

        var path = Path.Combine(directory.FullName, deploymentConfigFileName);
        
        if (!File.Exists(path))
        {
            content = null;
            return false;
        }
        
        content = File.ReadAllText(path, Encoding.UTF8);
        return true;
    }
    
    private static bool TryParseConfiguration(string configurationText, [NotNullWhen(true)] out FolderConfigDataHead? configuration)
    {
        Log.Debug("Trying to parse the configuration text into a custom model of type {ModelType}", 
            typeof(FolderConfigDataHead).FullName);
        
        DiagnosticsBag? diagnostics;
        
        var options = new TomlModelOptions
        {
            // IgnoreMissingProperties = true,
            ConvertPropertyName = ConvertPropertyName 
        };

        if (Toml.TryToModel<FolderConfigDataHead>(configurationText,
                out var parsedConfiguration,
                out diagnostics,
                null,
                options))
        {
            configuration = parsedConfiguration;
            return true;
        }

        Log.Error("Failed to parse the configuration text into a custom model of type {ModelType}", 
            typeof(FolderConfigDataHead).FullName);

        foreach (var message in diagnostics)
        {
            Log.Error("TOML parser diagnostic message of type {MessageType}: {MessageText}", 
                message.Kind, 
                message.Message);
        }

        configuration = null;
        return false;
    }

    // Convert the property name for matching with the TOML key
    private static string ConvertPropertyName(string name)
    {
        return name;
        
        // if (string.IsNullOrWhiteSpace(name))
        //     return name;
        //
        // var sb = new StringBuilder(name.Length);
        // var isFirstLetter = true;
        //
        // foreach (var ch in name.Trim())
        // {
        //     if (ch is '_' or '-' or ' ')
        //         isFirstLetter = true;
        //     else if (isFirstLetter)
        //     {
        //         sb.Append(char.ToUpperInvariant(ch));
        //         isFirstLetter = false;
        //     }
        //     else sb.Append(ch);
        // }
        //
        // Console.WriteLine($"TOML convert property name {name} -> {sb.ToString()}");
        // return sb.ToString();
    }

    private static RuleToCopyUnconditional BuildDefaultRuleCopyAll(DeploymentDefaults defaults, int index)
    {
        return new RuleToCopyUnconditional(
            Index: index,
            FileType: defaults.FileTypeDefault,
            SourceFileEncoding: defaults.SourceFileEncodingDefault,
            TargetFileEncoding: defaults.TargetFileEncodingDefault,
            FilePermission: FilePermission.UseDefault,
            LineEnding: defaults.LineEndingDefault,
            FixTrailingNewline: defaults.TrailingNewlineDefault);
    }

    private static RuleToSkipUnconditional BuildDefaultRuleSkipAll(int index)
    {
        return new RuleToSkipUnconditional(index);
    }
}