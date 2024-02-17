namespace vigoconfig;

internal record SourceBlockRule(IReadOnlyList<SourceLine> Lines, string Content) : SourceBlock(Lines, Content)
{
    public override string Description => $"Rule definition at lines {FirstLineNumber}..{LastLineNumber}";
}