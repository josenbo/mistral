using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigorule;

internal record FileRuleMatchingLiteral(
    FileRuleId Id,
    FileRuleActionEnum Action,
    string NameToMatch, 
    string NameReplacement,
    FileHandlingParameters Handling
) : FileRuleConditional(
    Id,
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

        transformation = new DeploymentTransformationFile(file, Handling, this)
        {
            CanDeploy = Action is FileRuleActionEnum.DeployFile,
            DifferentTargetFileName = newName
        };
        return true;
    }
}