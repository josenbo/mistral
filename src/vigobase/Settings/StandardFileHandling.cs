namespace vigobase;

public record StandardFileHandling(FileHandlingParameters Handling, bool DoCopy)
{
    public bool DoSkip => !DoCopy;
}