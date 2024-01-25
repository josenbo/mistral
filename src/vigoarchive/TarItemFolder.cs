using System.Formats.Tar;
using vigobase;

namespace vigoarchive;

public class TarItemFolder : TarItem
{
    protected internal TarItemFolder(string name, IEnumerable<TarItemFolder> parentFolderSequence, string relativePathAndName) : base(name, parentFolderSequence, relativePathAndName)
    {
    }

    public IEnumerable<TarItemFolder> TarItemFolders => Folders.Values;
    public IEnumerable<TarItemFile> TarItemFiles => Files.Values;

    internal override void SaveToTarball(TarWriter tarWriter)
    {
        var entry = new PaxTarEntry(TarEntryType.Directory, RelativePathAndName)
        {
            Uid = 0,
            UserName = "root",
            Gid = 0,
            GroupName = "root",
            Mode = (UnixFileMode)0b_111_101_101,
            ModificationTime = ModificationTime
        };
        tarWriter.WriteEntry(entry);
    }

    internal readonly Dictionary<string, TarItemFolder> Folders =
        new Dictionary<string, TarItemFolder>(StringComparer.Ordinal);
    internal readonly Dictionary<string, TarItemFile> Files =
        new Dictionary<string, TarItemFile>(StringComparer.Ordinal);
}
