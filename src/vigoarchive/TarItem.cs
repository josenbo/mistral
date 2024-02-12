namespace vigoarchive;

public abstract record TarItem(
    string TarRelativePath,
    UnixFileMode FileMode,
    DateTimeOffset ModificationTime
)
{
    public abstract IEnumerable<string> FolderPaths { get; }
    public string UnixPath { get; } = string.Join(TarPathSeparator, TarRelativePath.Split(PathSeparators)); 

    protected readonly string[] ItemArray = TarRelativePath.Split(PathSeparators);
    private static readonly char[] PathSeparators = ['\\', '/'];
    protected const char TarPathSeparator = '/';
}