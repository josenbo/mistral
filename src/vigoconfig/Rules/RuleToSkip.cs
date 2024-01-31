namespace vigoconfig;

internal abstract record RuleToSkip(int Index) : Rule(Index);