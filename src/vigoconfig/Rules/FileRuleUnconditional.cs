using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal record FileRuleUnconditional(
    int Index,
    FileRuleActionEnum Action,
    FileHandlingParameters Handling
) : FileRule(
    Index,
    Action,
    Handling
)
{
    internal override FileRuleConditionEnum Condition => FileRuleConditionEnum.Unconditional;
    internal override bool GetTransformation(FileInfo file, [NotNullWhen(true)] out IDeploymentTransformationReadWriteFile? transformation)
    {
        transformation = new DeploymentTransformationFile(file, Handling)
        {
            CanDeploy = Action is FileRuleActionEnum.CopyRule || (Action is FileRuleActionEnum.CheckRule && Handling.Settings.IsCommitCheck),
            DifferentTargetFileName = string.Empty
        };
        return true;
    }
}