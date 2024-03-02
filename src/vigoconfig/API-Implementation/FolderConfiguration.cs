using vigobase;

namespace vigoconfig;

internal class FolderConfiguration : IFolderConfiguration
{
    bool IFolderConfiguration.KeepEmptyFolder => _partialPartialFolderConfig.KeepEmptyFolder ?? false;
    ConfigurationFileTypeEnum IFolderConfiguration.ConfigurationType => _partialPartialFolderConfig.ConfigurationType;
    IConfigurationScriptExtract? IFolderConfiguration.BasedOn => _partialPartialFolderConfig.Block;
    
    IEnumerable<IFileRuleConfiguration> IFolderConfiguration.RuleConfigurations => _rules;

    IEnumerable<FileHandlingParameters> IFolderConfiguration.GetHandlingDefaultsChain()
    {
        yield return _initialDefaults;
    }

    FileHandlingParameters IFolderConfiguration.FolderDefaults => _folderDefaults;

    public FolderConfiguration(
        FileHandlingParameters initialDefaults,
        PartialFolderConfig partialPartialFolderConfig)
    {
        _initialDefaults = initialDefaults;
        _partialPartialFolderConfig = partialPartialFolderConfig;
        
        var handlingDefaultsChain = new List<FileHandlingParameters>();
        
        if (_partialPartialFolderConfig.LocalDefaults is null)
        {
            _folderDefaults = initialDefaults;
            handlingDefaultsChain.Add(_initialDefaults);
        }
        else
        {
            _folderDefaults = _partialPartialFolderConfig.LocalDefaults.Apply(_initialDefaults);
            if (_folderDefaults != _initialDefaults)
                handlingDefaultsChain.Add(_folderDefaults);
            handlingDefaultsChain.Add(initialDefaults);
        }

        foreach (var partialRule in _partialPartialFolderConfig.PartialRules)
        {
            _rules.Add(new FileRuleConfiguration(partialRule, _folderDefaults, handlingDefaultsChain));    
        }
    }

    private readonly List<IFileRuleConfiguration> _rules = [];
    private readonly FileHandlingParameters _initialDefaults;
    private readonly FileHandlingParameters _folderDefaults;
    private readonly PartialFolderConfig _partialPartialFolderConfig;
}