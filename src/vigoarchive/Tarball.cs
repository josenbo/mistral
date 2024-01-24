using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using Ardalis.GuardClauses;
using Serilog;

namespace vigoarchive;

public class Tarball(string basePath, string? additionalRootFolder = null)
{
    public TarItemFile AddFile(FileInfo file)
    {
        Log.Debug("Adding the file {TheFile} to the tarball", file);
        
        if (file.DirectoryName is null ||
            !file.DirectoryName.StartsWith(_basePath, StringComparison.Ordinal))
        {
            Log.Error("The file {TheFile} cannot be added to the tarball, because it lies outside the base path {TheBasePath}",
                file,
                _basePath);
            throw new ArgumentException("The file cannot be added to the tarball, because it lies outside the base path", nameof(file));
        }

        var relativePath = _additionalRootFolder + Path.GetRelativePath(_basePath, file.DirectoryName);
        var parentFolders = relativePath.Split( PathSeparators, StringSplitOptions.RemoveEmptyEntries);

        var sb = new StringBuilder(file.FullName.Length);
        var pf = new List<TarItemFolder>();
        var currentFolder = _anchor;
        var first = true;
        
        foreach (var parentFolder in parentFolders)
        {
            if (first)
            {
                sb.Append('/');
                first = false;
            }
            sb.Append(parentFolder);

            if (!currentFolder.Folders.TryGetValue(parentFolder, out var folder))
            {
                folder = new TarItemFolder(parentFolder, pf, sb.ToString());
                currentFolder.Folders.Add(parentFolder, folder);
                _contents.Add(folder);
            }

            currentFolder = folder;

            pf.Add(currentFolder);
        }
        
        sb.Append('/').Append(file.Name);

        var tarItemFile = new TarItemFile(file, file.Name, pf, sb.ToString())
        {
            ModificationTime = DateTimeOffset.FromFileTime(file.LastWriteTime.ToFileTimeUtc())
        };
        
        currentFolder.Files.Add(file.Name, tarItemFile);
        _contents.Add(tarItemFile);

        return tarItemFile;
    }
    
    public bool Save(FileInfo archiveFile)
    {
        archiveFile.Refresh();
        if (archiveFile.Exists)
            archiveFile.Delete();

        using var fileStream = archiveFile.OpenWrite();
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
        using var tarWriter = new TarWriter(gzipStream);
        
        foreach (var content in _contents.OrderBy(ti => ti.RelativePathAndName))
        {
            content.SaveToTarball(tarWriter);
        }

        return true;
    }
    
    private readonly List<TarItem> _contents = [];
    private readonly TarItemFolder _anchor = new TarItemFolder("anchor", Array.Empty<TarItemFolder>(), string.Empty);
    private readonly string _basePath  = Guard.Against.InvalidInput(basePath, nameof(basePath), s => !string.IsNullOrWhiteSpace(s) && Directory.Exists(s));
    private readonly string _additionalRootFolder = string.IsNullOrWhiteSpace(additionalRootFolder) 
        ? string.Empty 
        : additionalRootFolder + "/";
    private static readonly char[] PathSeparators = ['\\', '/'];
}
