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
    internal override bool GetTransformation(FileInfo file,
        DeploymentDefaults defaults,
        [NotNullWhen(true)] out IDeploymentTransformationReadWriteFile? transformation)
    {
        if (!NameToMatch.Equals(file.Name, StringComparison.Ordinal))
        {
            transformation = null;
            return false;
        }

        transformation = new DeploymentTransformationFile(file, defaults)
        {
            CanDeploy = false
        };
        return true;
    }
}