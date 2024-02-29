namespace vigoconfig;

internal abstract record SourceBlock(
    IReadOnlyList<SourceLine> Lines, 
    string Content, 
    string ConfigurationFile, 
    int Position
    ) : IConfigurationScriptExtract
{
    public int FromLineNumber => Lines[0].LineNumber;
    public int ToLineNumber => Lines[^1].LineNumber;
    public abstract string Description { get; }
}