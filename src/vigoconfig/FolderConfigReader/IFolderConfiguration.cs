using vigobase;

namespace vigoconfig;

internal interface IFolderConfiguration
{
    DeploymentDefaults GlobalDefaults { get; }
    DeploymentDefaults LocalDefaults { get; }
    DirectoryInfo Location { get; }
    void SetLocalDefaults(DeploymentDefaults localDefaults);
    bool HasKeepFolderFlag { get; set; }
    int NextRuleIndex { get; }
    void AddRule(Rule rule);
}