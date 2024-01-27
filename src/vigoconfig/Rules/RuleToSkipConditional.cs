namespace vigoconfig;

internal abstract record RuleToSkipConditional(
    string NameToMatch, 
    string NameReplacement
) : RuleToSkip();