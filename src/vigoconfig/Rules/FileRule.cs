using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal abstract record FileRule(
    int Index,
    FileRuleActionEnum Action,
    FileHandlingParameters Handling
) 
{
    internal abstract bool GetTransformation(FileInfo file, [NotNullWhen(true)] out IDeploymentTransformationReadWriteFile? transformation);
    internal virtual bool DoCheck => Action is FileRuleActionEnum.CheckRule or FileRuleActionEnum.CopyRule;
    internal virtual bool DoCopy => Action is FileRuleActionEnum.CopyRule;
    internal virtual bool DoSkip => Action is FileRuleActionEnum.SkipRule;
}