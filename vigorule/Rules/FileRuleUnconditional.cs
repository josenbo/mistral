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
    internal override bool GetTransformation(FileInfo file, bool includePreview,
        [NotNullWhen(true)] out IMutableFileHandling? transformation)
    {
        transformation = new FileHandlingImpl(file, Handling, this)
        {
            CanDeploy = (Action == FileRuleActionEnum.DeployFile || (includePreview && Action ==FileRuleActionEnum.PreviewFile)),
            DifferentTargetFileName = string.Empty
        };
        return true;
    }
}