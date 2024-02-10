using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal record FileRuleMatchingLiteral(
    int Index,
    FileRuleActionEnum Action,
    string NameToMatch, 
    string NameReplacement,
    FileHandlingParameters Handling
) : FileRuleConditional(
    Index,
    Action,
    NameToMatch, 
    NameReplacement,
    Handling
)
{
    internal override FileRuleConditionEnum Condition => FileRuleConditionEnum.MatchName;
    internal override bool GetTransformation(FileInfo file, [NotNullWhen(true)] out IDeploymentTransformationReadWriteFile? transformation)
    {
        if (!NameToMatch.Equals(file.Name, StringComparison.Ordinal))
        {
            transformation = null;
            return false;
        }

        var newName = string.IsNullOrWhiteSpace(NameReplacement)
            ? string.Empty
            : NameReplacement;

        transformation = new DeploymentTransformationFile(file, Handling)
        {
            CanDeploy = Action is FileRuleActionEnum.CopyRule || (Action is FileRuleActionEnum.CheckRule && Handling.Settings.IsCommitCheck),
            DifferentTargetFileName = newName
        };
        return true;
    }
}