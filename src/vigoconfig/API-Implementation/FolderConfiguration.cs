using vigobase;

namespace vigoconfig;

internal class FolderConfiguration : IFolderConfiguration
{
    bool IFolderConfiguration.KeepEmptyFolder => _partialFolderConfig.KeepEmptyFolder ?? false;

    IConfigurationScriptExtract? IFolderConfiguration.BasedOn => _partialFolderConfig.Block;
    
    IEnumerable<IFileRuleConfiguration> IFolderConfiguration.RuleConfigurations => _rules;

    IEnumerable<FileHandlingParameters> IFolderConfiguration.GetHandlingDefaultsChain()
    {
        yield return _initialDefaults;
    }

    FileHandlingParameters IFolderConfiguration.FolderDefaults => _folderDefaults;

    public FolderConfiguration(
        FileHandlingParameters initialDefaults,
        FolderConfig partialFolderConfig)
    {
        _initialDefaults = initialDefaults;
        _partialFolderConfig = partialFolderConfig;
        
        var handlingDefaultsChain = new List<FileHandlingParameters>();
        
        if (_partialFolderConfig.LocalDefaults is null)
        {
            _folderDefaults = initialDefaults;
            handlingDefaultsChain.Add(_initialDefaults);
        }
        else
        {
            _folderDefaults = _partialFolderConfig.LocalDefaults.Apply(_initialDefaults);
            if (_folderDefaults != _initialDefaults)
                handlingDefaultsChain.Add(_folderDefaults);
            handlingDefaultsChain.Add(initialDefaults);
        }

        foreach (var partialRule in _partialFolderConfig.PartialRules)
        {
            _rules.Add(new FileRuleConfiguration(partialRule, _folderDefaults, handlingDefaultsChain));    
        }
    }

    private readonly List<IFileRuleConfiguration> _rules = [];
    private readonly FileHandlingParameters _initialDefaults;
    private readonly FileHandlingParameters _folderDefaults;
    private readonly FolderConfig _partialFolderConfig;
}