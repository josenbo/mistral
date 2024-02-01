using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using vigobase;

namespace vigoconfig;

internal record RuleToSkipMatchingPattern(
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
        if (!Regex.IsMatch(file.Name, NameToMatch))
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