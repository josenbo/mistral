using JetBrains.Annotations;
using vigobase;

namespace vigoconfig;

[PublicAPI]
public record PartialFileRule(
    FileRuleActionEnum Action, 
    FileRuleConditionEnum Condition,
    string? CompareWith,
    string? ReplaceWith,
    PartialFileHandlingParameters? Handling);