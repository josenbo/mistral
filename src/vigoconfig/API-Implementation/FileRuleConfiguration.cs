using vigobase;

namespace vigoconfig;

internal class FileRuleConfiguration : IFileRuleConfiguration
{
    int IFileRuleConfiguration.RuleIndex => _partialRule.Block.RuleIndex;

    string IFileRuleConfiguration.RuleDescription => _partialRule.Description;

    FileRuleActionEnum IFileRuleConfiguration.Action => _partialRule.Action;

    FileRuleConditionEnum IFileRuleConfiguration.Condition => _partialRule.Condition;

    string? IFileRuleConfiguration.CompareWith => _partialRule.CompareWith;

    string? IFileRuleConfiguration.ReplaceWith => _partialRule.ReplaceWith;

    FileHandlingParameters IFileRuleConfiguration.Handling => _handling;

    bool IFileRuleConfiguration.IsExplicitelyDefined => true;

    IConfigurationScriptExtract IFileRuleConfiguration.BasedOn => _partialRule.Block;

    IEnumerable<FileHandlingParameters> IFileRuleConfiguration.GetHandlingDefaultsChain() => _handlingDefaultsChain;

    public FileRuleConfiguration(FolderConfigPartialRule partialRule, FileHandlingParameters folderDefaults, List<FileHandlingParameters> handlingDefaultsChain)
    {
        _partialRule = partialRule;
        _handlingDefaultsChain = handlingDefaultsChain;
        _handling = _partialRule.Handling is null 
            ? folderDefaults 
            : _partialRule.Handling.Apply(folderDefaults);
    }

    private readonly FolderConfigPartialRule _partialRule;
    private readonly FileHandlingParameters _handling;
    private readonly List<FileHandlingParameters> _handlingDefaultsChain;
}