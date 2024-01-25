using System.Formats.Tar;
using System.Text;

namespace vigoarchive;

public class TarItemFile : TarItem
{
    protected internal TarItemFile(FileInfo file, string name, IEnumerable<TarItemFolder> parentFolderSequence, string relativePathAndName) : base(name, parentFolderSequence, relativePathAndName)
    {
        _fileInfo = file;
    }

    internal override void SaveToTarball(TarWriter tarWriter)
    {
        var entry = new PaxTarEntry(TarEntryType.RegularFile, RelativePathAndName)
        {
            Uid = 0,
            UserName = "root",
            Gid = 0,
            GroupName = "root",
            Mode = (UnixFileMode)0b_110_100_000,
            ModificationTime = ModificationTime
        };
        //using var fileStream = _fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        //entry.DataStream = fileStream;
        //tarWriter.WriteEntry(entry);

        using var stream = new MemoryStream();
        using var sw = new StreamWriter(stream);
        sw.Write("Hi there, Tar!");
        sw.Flush();
        stream.Position = 0;
        entry.DataStream = stream;
        tarWriter.WriteEntry(entry);
    }

    private readonly FileInfo _fileInfo;
}
