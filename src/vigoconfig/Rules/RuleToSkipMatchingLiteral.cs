using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal record RuleToSkipMatchingLiteral(
    int Index,
    string NameToMatch
) : RuleToSkipConditional(
    Index,
    NameToMatch
)
{
    internal override bool GetTransformation(string filename, out RuleCheckResultEnum result,
        [NotNullWhen(true)] out IDeploymentTransformationReadWrite? transformation)
    {
        throw new NotImplementedException();
    }
}