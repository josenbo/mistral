namespace vigoconfig;

internal class CommonDefinitions
{
    public Dictionary<string, NamedFileList> NamedFileListDict { get; } =
        new Dictionary<string, NamedFileList>(StringComparer.InvariantCultureIgnoreCase);
}