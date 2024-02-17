using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigorule;

internal record FileRuleUnconditional(
    FileRuleId Id,
    FileRuleActionEnum Action,
    FileHandlingParameters Handling
) : FileRule(
    Id,
    Action,
    Handling
)
{
    internal override FileRuleConditionEnum Condition => FileRuleConditionEnum.Unconditional;
    internal override bool GetTransformation(FileInfo file, [NotNullWhen(true)] out IDeploymentTransformationReadWriteFile? transformation)
    {
        transformation = new DeploymentTransformationFile(file, Handling, this)
        {
            CanDeploy = Action is FileRuleActionEnum.CopyRule,
            DifferentTargetFileName = string.Empty
        };
        return true;
    }
}