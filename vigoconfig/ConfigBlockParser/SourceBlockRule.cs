namespace vigoconfig;

internal record SourceBlockRule(
    IReadOnlyList<SourceLine> Lines, 
    string Content, 
    string ConfigurationFile, 
    int Position,
    int RuleIndex
    ) : SourceBlock(Lines, Content, ConfigurationFile, Position)
{
    public override string Description => $"Rule definition #{Position} at lines {FromLineNumber}..{ToLineNumber} of {ConfigurationFile}";
}