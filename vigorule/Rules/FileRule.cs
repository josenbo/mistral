using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigorule;

internal abstract record FileRule(
    FileRuleId Id,
    FileRuleActionEnum Action,
    FileHandlingParameters Handling
) 
{
    internal abstract FileRuleConditionEnum Condition { get; }
    internal abstract bool GetTransformation(
        FileInfo file,
        bool includePreview,
        [NotNullWhen(true)] out IMutableFileHandling? transformation);
}