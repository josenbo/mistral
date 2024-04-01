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
    internal override bool GetTransformation(
        FileInfo file, 
        bool includePreview,
        [NotNullWhen(true)] out IMutableFileHandling? transformation)
    {
        if (!NameToMatch.Equals(file.Name, StringComparison.Ordinal))
        {
            transformation = null;
            return false;
        }

        var newName = string.IsNullOrWhiteSpace(NameReplacement)
            ? string.Empty
            : NameReplacement;

        transformation = new FileHandlingImpl(file, Handling, this)
        {
            CanDeploy = (Action == FileRuleActionEnum.DeployFile || (includePreview && Action ==FileRuleActionEnum.PreviewFile)),
            DifferentTargetFileName = newName
        };
        return true;
    }
}