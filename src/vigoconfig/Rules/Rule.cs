using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal abstract record Rule(int Index)
{
    internal abstract bool GetTransformation(FileInfo file, DeploymentDefaults defaults,
        [NotNullWhen(true)] out IDeploymentTransformationReadWriteFile? transformation);
    internal virtual bool IsCopyRule => this is RuleToCopy;
    internal virtual bool IsSkipRule => this is RuleToSkip;
}