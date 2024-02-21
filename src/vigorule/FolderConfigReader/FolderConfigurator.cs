using System.Diagnostics.CodeAnalysis;
using System.Text;
using Serilog;
using Tomlyn;
using vigobase;

namespace vigorule;

internal static class FolderConfigurator
{
    public static void Configure(IFolderConfiguration config)
    {
        var appSettings = config.ParentFileHandlingParams.Settings;
        var relativePath = appSettings.GetRepoRelativePath(config.Location);
        
        try
        {
            // ToDo - support for alternative file names (deployment.md or deployment.vigo) 
            const string theConfigfile = "todo-need-to-fix-this";
            
            // config.AddRule(BuildDeployConfigFileRule(relativePath, config.NextRuleIndex, appSettings.DeploymentConfigFileName, appSettings.DeployConfigRule));
            config.AddRule(BuildDeployConfigFileRule(relativePath, config.NextRuleIndex, theConfigfile, appSettings.DeployConfigRule));
            
            // if (!TryReadFileContent(config.Location, config.ParentFileHandlingParams.Settings.DeploymentConfigFileName, out var content))
            if (!TryReadFileContent(config.Location, theConfigfile, out var content))
            {
                config.AddRule(BuildFinalCatchAllFileRule(relativePath, config.NextRuleIndex, appSettings.FinalCatchAllRule));
                return;
            }
            
            Log.Information("Found a deployment configuration file in the directory {TheDirectory}",
                appSettings.GetRepoRelativePath(config.Location.FullName));
        
            if (!TryParseConfiguration(content, out var tomlConfigurationData))
            {
                Log.Fatal("Could not read the configuration file {TheFileName}",
                    theConfigfile);
                    //appSettings.DeploymentConfigFileName);

                throw new VigoFatalException("Could not read the folder configuration");
            }

            config.HasKeepFolderFlag = tomlConfigurationData.KeepEmptyFolder is true;
            config.SetLocalDefaults(tomlConfigurationData.GetLocalDefaults(config.ParentFileHandlingParams));
                
            ValidateAndAppendRules(config, tomlConfigurationData.Rules);

            config.AddRule(BuildFinalCatchAllFileRule(relativePath, config.NextRuleIndex, appSettings.FinalCatchAllRule));
        }
        catch (Exception e) when (e is not VigoException)
        {
            Log.Fatal(e,"Could not read the configuration in the directory {TheDir}",
                appSettings.GetRepoRelativePath(config.Location.FullName));
            throw new VigoFatalException("Failed to read the deployment configuration in the repository folder");
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

        var options = new TomlModelOptions
        {
            // IgnoreMissingProperties = true,
            ConvertPropertyName = ConvertPropertyName 
        };

        if (Toml.TryToModel<FolderConfigDataHead>(configurationText,
                out var parsedConfiguration,
                out var diagnostics,
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

    private static void ValidateAndAppendRules(IFolderConfiguration config, IEnumerable<FolderConfigDataRule> tomlRuleData)
    {
        foreach (var data in tomlRuleData.Select(d => d.GetFileRule(config, config.LocalFileHandlingParams)))
        {
            config.AddRule(data);
        }    
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

    private static FileRuleMatchingLiteral BuildDeployConfigFileRule(string repoDirectory, int index, string deploymentConfigFileName, StandardFileHandling ruleConfig)
    {
        return new FileRuleMatchingLiteral(
            Id: new FileRuleId(repoDirectory, index, $"[Predefined initial rule] {(ruleConfig.DoCopy ? "Copy" : "Skip")} when name equals '{deploymentConfigFileName}'"),
            Action: ruleConfig.DoCopy ? FileRuleActionEnum.CopyRule : FileRuleActionEnum.SkipRule,
            NameToMatch: deploymentConfigFileName,
            Handling: ruleConfig.Handling,
            NameReplacement: string.Empty
            );
    }
    
    private static FileRuleUnconditional BuildFinalCatchAllFileRule(string repoDirectory, int index, StandardFileHandling ruleConfig)
    {
        return new FileRuleUnconditional(
            Id: new FileRuleId(repoDirectory, index, $"[Predefined final catch-all rule] {(ruleConfig.DoCopy ? "Copy" : "Skip")} unconditionally"),
            Action: ruleConfig.DoCopy ? FileRuleActionEnum.CopyRule : FileRuleActionEnum.SkipRule,
            Handling: ruleConfig.Handling
            );
    }
}