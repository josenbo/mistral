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
    internal override bool GetTransformation(FileInfo file, [NotNullWhen(true)] out IMutableFileHandling? transformation)
    {
        transformation = new FileHandlingImpl(file, Handling, this)
        {
            CanDeploy = Action is FileRuleActionEnum.DeployFile,
            DifferentTargetFileName = string.Empty
        };
        return true;
    }
}