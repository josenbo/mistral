namespace vigobase;

public record StandardFileHandling(IReadOnlyList<ConfigurationFilename> Filenames, FileHandlingParameters Handling, bool DoCopy)
{
    public bool DoSkip => !DoCopy;
}