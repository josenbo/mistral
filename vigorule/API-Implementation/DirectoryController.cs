using Serilog;
using vigobase;
using vigoconfig;

namespace vigorule;

internal class DirectoryController(DirectoryInfo location, RepositoryReadRequest request, bool isTopLevelDirectory)
{
    public DirectoryInfo Location { get; } = location;

    public bool IsTopLevelDirectory { get; } = isTopLevelDirectory;

    private RepositoryReadRequest Request { get; } = request;

    public IMutableDirectoryHandling GetDirectoryTransformation()
    {
        return new DirectoryHandlingImpl(Location, _folderConfiguration?.KeepEmptyFolder ?? false, IsTopLevelDirectory, GetTargets);
    }
    
    public IMutableFileHandling GetFileTransformation(FileInfo file)
    {
        if (_rules.Count == 0)
            AppendRules(_rules, _folderConfiguration, Location, Request);

        foreach (var rule in _rules)
        {
            if (rule.GetTransformation(file, Request.IncludePreview, out var transformation))
                return transformation;
        }
        
        Log.Fatal("There is no rule matching the file name {TheFileName} in the directory {TheDirectory}",
            file.Name,
            AppEnv.GetTopLevelRelativePath(file.FullName));
        throw new VigoFatalException(AppEnv.Faults.Fatal(
            "FX560",
            "Check why the final catch-all rule was missing or did not handle this file", 
            string.Empty));
    }

    private readonly IFolderConfiguration? _folderConfiguration = ReadFolderConfiguration(location, request);
    private readonly List<FileRule> _rules = [];

    private static IFolderConfiguration? ReadFolderConfiguration(
        DirectoryInfo location, 
        RepositoryReadRequest request)
    {
        foreach (var configFile in request.ConfigFiles)
        {
            var configurationFile = new FileInfo(Path.Combine(location.FullName, configFile.FileName));

            if (!configurationFile.Exists)
                continue;

            var configurationScript = configurationFile.OpenText().ReadToEnd();

            if (FolderConfigurationApi.Reader.TryParse(
                    configurationScript: configurationScript,
                    configurationFile: request.GetTopLevelRelativePath(configurationFile),
                    configurationType: configFile.FileType,
                    initialDefaults: request.DefaultHandling,
                    out var folderConfiguration
                ))
                return folderConfiguration;
        }

        return null;
    }

    private static void AppendRules(
        List<FileRule> rules, 
        IFolderConfiguration? folderConfiguration,
        DirectoryInfo location, 
        RepositoryReadRequest request)
    {
        var repoDirectory = request.GetTopLevelRelativePath(location);

        var configFilePattern = request.GetConfigFilesRegexPattern(); 
        
        rules.Add(new FileRuleMatchingPattern(
            Id: new FileRuleId(repoDirectory,rules.Count, $"[Predefined initial rule] Ignore when name matches '{configFilePattern}'"),
            Action: FileRuleActionEnum.IgnoreFile,
            NameToMatch: configFilePattern,
            NameReplacement: string.Empty,
            Handling: request.IgnoredFileHandling));

        if (folderConfiguration is not null)
        {
            foreach (var ruleConfiguration in folderConfiguration.RuleConfigurations.OrderBy(cf => cf.RuleIndex))
            {
                FileRule rule = ruleConfiguration.Condition switch
                {
                    FileRuleConditionEnum.Unconditional => new FileRuleUnconditional(
                        Id: new FileRuleId(repoDirectory, rules.Count, ruleConfiguration.RuleDescription),
                        Action: ruleConfiguration.Action,
                        Handling: ruleConfiguration.Handling),
                    
                    FileRuleConditionEnum.MatchName => new FileRuleMatchingLiteral(
                        Id: new FileRuleId(repoDirectory, rules.Count, ruleConfiguration.RuleDescription),
                        NameToMatch: ruleConfiguration.CompareWith ?? throw new VigoFatalException(AppEnv.Faults.Fatal(
                            "FX119",
                            "Supposed to be checked and not null", 
                            string.Empty)),
                        NameReplacement: ruleConfiguration.ReplaceWith ?? string.Empty,
                        Action: ruleConfiguration.Action,
                        Handling: ruleConfiguration.Handling),

                    FileRuleConditionEnum.MatchPattern => new FileRuleMatchingPattern(
                        Id: new FileRuleId(repoDirectory, rules.Count, ruleConfiguration.RuleDescription),
                        NameToMatch: ruleConfiguration.CompareWith ?? throw new VigoFatalException(AppEnv.Faults.Fatal(
                            "FX126",
                            "Supposed to be checked and not null", 
                            string.Empty)),
                        NameReplacement: ruleConfiguration.ReplaceWith ?? string.Empty,
                        Action: ruleConfiguration.Action,
                        Handling: ruleConfiguration.Handling),
                    
                    FileRuleConditionEnum.MatchHandler => new FileRuleMatchingHandler(
                        Id: new FileRuleId(repoDirectory, rules.Count, ruleConfiguration.RuleDescription),
                        Action: ruleConfiguration.Action,
                        NameTestAndReplaceHandler: ruleConfiguration.NameTestAndReplaceHandler ?? throw new VigoFatalException(AppEnv.Faults.Fatal(
                            "FX616", 
                            $"Existence of a {nameof(INameTestAndReplaceHandler)} implementation must be checked and ensured during rule compilation",
                            string.Empty)),
                        Handling: ruleConfiguration.Handling),
                    
                    _ => throw new VigoFatalException(AppEnv.Faults.Fatal(
                        "FX133",  
                        "Are we missing a new rule type", 
                        string.Empty))
                };
                
                rules.Add(rule);
            }
        }
        
        rules.Add(new FileRuleUnconditional(
            Id: new FileRuleId(repoDirectory, rules.Count, ""),
            Action: FileRuleActionEnum.IgnoreFile,
            Handling: request.IgnoredFileHandling));
    }

    internal IEnumerable<string> GetTargets()
    {
        return _deploymentTargetDictionary.Keys;
    }

    internal void CollectDeploymentTargets(IEnumerable<string> targets)
    {
        foreach (var target in targets)
        {
            _deploymentTargetDictionary.TryAdd(target, target);
        }
    }
    
    private readonly Dictionary<string, string> _deploymentTargetDictionary =
        new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
}
