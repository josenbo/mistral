using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal record RuleToSkipUnconditional(int Index) : RuleToSkip(Index)
{
    internal override bool GetTransformation(FileInfo file,
        DeploymentDefaults defaults,
        [NotNullWhen(true)] out IDeploymentTransformationReadWriteFile? transformation)
    {
        transformation = new DeploymentTransformationFile(file, defaults)
        {
            CanDeploy = false
        };
        return true;
    }
}