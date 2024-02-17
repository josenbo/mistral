namespace vigoconfig;

internal abstract record SourceBlock(IReadOnlyList<SourceLine> Lines, string Content)
{
    public int FirstLine => Lines[0].LineNumber;
    public int LastList => Lines[^1].LineNumber;
    public abstract string Description { get; }
}