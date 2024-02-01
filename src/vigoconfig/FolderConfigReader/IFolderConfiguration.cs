using vigobase;

namespace vigoconfig;

internal interface IFolderConfiguration
{
    DeploymentDefaults Defaults { get; }
    DirectoryInfo Location { get; }
    bool HasKeepFolderFlag { get; set; }
    int NextRuleIndex { get; }
    void AddRule(Rule rule);
}