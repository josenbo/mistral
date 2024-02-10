using vigobase;

namespace vigoconfig;

internal abstract record FileRuleConditional(
    int Index,
    FileRuleActionEnum Action,
    string NameToMatch, 
    string NameReplacement,
    FileHandlingParameters Handling
) : FileRule(
    Index,
    Action,
    Handling
);