using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal record RuleToSkipMatchingLiteral(
    string NameToMatch,
    string NameReplacement
) : RuleToSkipConditional(
    NameToMatch,
    NameReplacement
)
{
    internal override bool GetTransformation(string filename, out RuleCheckResultEnum result,
        [NotNullWhen(true)] out IDeploymentTransformationReadWrite? transformation)
    {
        throw new NotImplementedException();
    }
}