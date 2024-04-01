namespace vigoconfig;

internal record SourceBlockDefine(
    IReadOnlyList<SourceLine> Lines,
    string Content, 
    string ConfigurationFile, 
    int Position
) : SourceBlock(Lines, Content, ConfigurationFile, Position)
{
    public override string Description => $"List definition #{Position} at lines {FromLineNumber}..{ToLineNumber} of {ConfigurationFile}";
}