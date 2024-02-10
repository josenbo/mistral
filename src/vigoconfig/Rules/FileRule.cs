using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal abstract record FileRule(
    int Index,
    FileRuleActionEnum Action,
    FileHandlingParameters Handling
) 
{
    internal abstract FileRuleConditionEnum Condition { get; }
    internal abstract bool GetTransformation(FileInfo file, [NotNullWhen(true)] out IDeploymentTransformationReadWriteFile? transformation);
}