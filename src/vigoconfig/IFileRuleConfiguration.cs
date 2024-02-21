using JetBrains.Annotations;
using vigobase;

namespace vigoconfig;

[PublicAPI]
public interface IFileRuleConfiguration
{
    FileRuleActionEnum Action { get; }
    FileRuleConditionEnum Condition { get; }
    string? CompareWith { get; } 
    string? ReplaceWith { get; }
    FileHandlingParameters Handling { get; } 
}