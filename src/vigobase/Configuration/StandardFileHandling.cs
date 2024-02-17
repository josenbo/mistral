namespace vigobase;

public record StandardFileHandling(IReadOnlyList<string> Filenames, FileHandlingParameters Handling, bool DoCopy)
{
    public bool DoSkip => !DoCopy;
}