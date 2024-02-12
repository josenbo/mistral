using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using vigobase;

namespace vigoconfig;

internal record FileRuleMatchingPattern(
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
    internal override FileRuleConditionEnum Condition => FileRuleConditionEnum.MatchPattern;
    internal override bool GetTransformation(FileInfo file, [NotNullWhen(true)] out IDeploymentTransformationReadWriteFile? transformation)
    {
        if (!Regex.IsMatch(file.Name, NameToMatch))
        {
            transformation = null;
            return false;
        }

        var newName = string.IsNullOrWhiteSpace(NameReplacement)
            ? string.Empty
            : Regex.Replace(file.Name, NameToMatch, NameReplacement);

        transformation = new DeploymentTransformationFile(file, Handling)
        {
            CanDeploy = Action is FileRuleActionEnum.CopyRule || (Action is FileRuleActionEnum.CheckRule && Handling.Settings.Command.IsCheckCommit()),
            DifferentTargetFileName = newName
        };
        return true;
    }
}