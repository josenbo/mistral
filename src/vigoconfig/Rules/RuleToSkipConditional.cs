namespace vigoconfig;

internal abstract record RuleToSkipConditional(
    int Index,
    string NameToMatch 
) : RuleToSkip(Index);