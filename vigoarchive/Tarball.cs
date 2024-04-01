using System.Formats.Tar;
using System.IO.Compression;
using JetBrains.Annotations;
using Serilog;

namespace vigoarchive;

[PublicAPI]
public class Tarball
{
    public UnixFileMode DirectoryFileMode { get; set; } = (UnixFileMode)0b_111_111_111;
    public DateTimeOffset DirectoryModificationTime { get; set; } = DateTimeOffset.Now;
    public void AddItem(TarItem item)
    {
        Log.Debug("Adding the item {TheItem} to the tarball", item);

        switch (item)
        {
            case TarItemDirectory:
                _itemDict.TryAdd(item.UnixPath, item);
                break;
            case TarItemFile:
                _itemDict.Add(item.UnixPath, item);
                break;
        }
        
        foreach (var folderPath in item.FolderPaths)
        {
            if (!_itemDict.ContainsKey(folderPath))
                _itemDict.Add(folderPath, new TarItemDirectory(
                    TarRelativePath: folderPath,
                    FileMode: DirectoryFileMode,
                    ModificationTime: DirectoryModificationTime
                    ));
        }
    }
    
    public bool Save(FileInfo archiveFile)
    {
        archiveFile.Refresh();
        if (archiveFile.Exists)
            archiveFile.Delete();

        using var fileStream = archiveFile.OpenWrite();
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
        using var tarWriter = new TarWriter(gzipStream);
        
        foreach (var item in _itemDict.Values.OrderBy(ti => ti.UnixPath))
        {
            
            
            switch (item)
            {
                case TarItemDirectory directory:
                    {
                        var entry = new PaxTarEntry(TarEntryType.Directory, directory.UnixPath)
                        {
                            Uid = 0,
                            UserName = "root",
                            Gid = 0,
                            GroupName = "root",
                            Mode = directory.FileMode,
                            ModificationTime = directory.ModificationTime
                        };

                        tarWriter.WriteEntry(entry);
                    }                    
                    break;

                case TarItemFile file:
                    {
                        var entry = new PaxTarEntry(TarEntryType.RegularFile, file.UnixPath)
                        {
                            Uid = 0,
                            UserName = "root",
                            Gid = 0,
                            GroupName = "root",
                            Mode = file.FileMode,
                            ModificationTime = file.ModificationTime
                        };
                        using var dataStream = file.TransformedContent.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                        entry.DataStream = dataStream;
                        tarWriter.WriteEntry(entry);
                    }
                    break;
            }
        }

        return true;
    }

    private readonly Dictionary<string, TarItem> _itemDict = new Dictionary<string, TarItem>();
}
