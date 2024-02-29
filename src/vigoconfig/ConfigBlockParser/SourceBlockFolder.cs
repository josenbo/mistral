namespace vigoconfig;

internal record SourceBlockFolder(
    IReadOnlyList<SourceLine> Lines,
    string Content, 
    string ConfigurationFile, 
    int Position
    ) : SourceBlock(Lines, Content, ConfigurationFile, Position)
{
    public override string Description => $"Folder configuration #{Position} at lines {FromLineNumber}..{ToLineNumber} of {ConfigurationFile}";
}