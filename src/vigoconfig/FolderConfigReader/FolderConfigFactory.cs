using System.Diagnostics.CodeAnalysis;
using System.Text;
using Ardalis.GuardClauses;
using Serilog;
using Tomlyn;
using Tomlyn.Syntax;
using vigobase;

namespace vigoconfig;

internal static class FolderConfigFactory
{
    public static FolderConfig Create(DirectoryInfo? directory, DeploymentDefaults defaults)
    {
        try
        {
            if (!TryReadFileContent(directory, defaults.DeploymentConfigFileName, out var content))
                return new FolderConfig() { DefaultActionSkip = true };
        
            if (!TryParseConfiguration(content, out var tomlConfigurationData))
            {
                Log.Fatal("Could not read the configuration file {TheFileName} in the directory {TheDirectory}",
                    defaults.DeploymentConfigFileName,
                    directory);

                throw new VigoFatalException("Could not read the folder configuration");
            }
        
            var folderConfig = new FolderConfig()
            {
                KeepEmptyFolder = tomlConfigurationData.KeepEmptyFolder is true,
                DefaultActionSkip = false
            };

            ValidateAndAppendRules(folderConfig, tomlConfigurationData.Rules, defaults);

            return folderConfig;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Log.Fatal(e,"Could not read the configuration in the directory {TheDir}",
                directory);
            throw new VigoFatalException("Failed to read the deployment configuration in the repository folder");
        }
    }

    private static void ValidateAndAppendRules(FolderConfig folderConfig, IEnumerable<FolderConfigDataRule> tomlRuleData, DeploymentDefaults defaults)
    {
        foreach (var data in tomlRuleData.Select(d => d.GetValidRuleData(defaults)))
        {
            switch (data.RuleType)
            {
                case RuleTypeEnum.CopyAll:
                    folderConfig.AddRule(new RuleToCopyUnconditional(
                        folderConfig.NextIndex, 
                        data.FileType,
                        data.SourceFileEncoding,
                        data.TargetFileEncoding,
                        data.FilePermission,
                        data.LineEnding,
                        data.FixTrailingNewline));
                    break;
                case RuleTypeEnum.CopyName:
                    folderConfig.AddRule(new RuleToCopyMatchingLiteral(
                        folderConfig.NextIndex,
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
                    folderConfig.AddRule(new RuleToCopyMatchingPattern(
                        folderConfig.NextIndex,
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
                    folderConfig.AddRule(new RuleToSkipUnconditional(folderConfig.NextIndex));
                    break;
                case RuleTypeEnum.SkipName:
                    folderConfig.AddRule(new RuleToSkipMatchingLiteral(folderConfig.NextIndex, data.SourceFileName));
                    break;
                case RuleTypeEnum.SkipPattern:
                    folderConfig.AddRule(new RuleToSkipMatchingPattern(folderConfig.NextIndex, data.SourceFileNamePattern));
                    break;
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
        Log.Debug("Trying to parsing the configuration text into a custom model of type {ModelType}", 
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
    
    private static string ConvertPropertyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;
	
        var sb = new StringBuilder(name.Length);
        var isFirstLetter = true;
	
        foreach (var ch in name.Trim())
        {
            if (isFirstLetter)
            {
                sb.Append(char.ToLowerInvariant(ch));
                isFirstLetter = false;
            }
            else sb.Append(ch);
        }
	
        return sb.ToString();
    }
}