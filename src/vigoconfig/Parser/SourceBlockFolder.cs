namespace vigoconfig;

internal record SourceBlockFolder(IReadOnlyList<SourceLine> Lines, string Content) : SourceBlock(Lines, Content)
{
    public override string Description => $"Folder configuration at lines {FirstLine}..{LastList}";
}