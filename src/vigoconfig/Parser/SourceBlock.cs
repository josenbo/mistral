namespace vigoconfig;

internal abstract record SourceBlock(IReadOnlyList<SourceLine> Lines, string Content)
{
    public int FirstLineNumber => Lines[0].LineNumber;
    public int LastLineNumber => Lines[^1].LineNumber;
    public abstract string Description { get; }
}