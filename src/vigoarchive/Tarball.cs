using System.Diagnostics.CodeAnalysis;
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
        
        var sb = new StringBuilder(file.FullName.Length + _additionalRootFolder.Length + 10);
        var parentFolderSequence = new List<TarItemFolder>();
        var currentFolder = _anchor;
        var first = true;
        
        foreach (var parentFolder in BuildTarballPathFolderSequence(file.DirectoryName))
        {
            if (first)
                first = false;
            else
                sb.Append(TarPathSeparator);

            sb.Append(parentFolder);

            if (!currentFolder.Folders.TryGetValue(parentFolder, out var folder))
            {
                var relativePathAndFolderName = sb.ToString();
                Log.Debug("Adding folder entry {FolderName} with path {RelativePathAndName}", 
                    parentFolder, 
                    relativePathAndFolderName);
                folder = new TarItemFolder(parentFolder, parentFolderSequence, relativePathAndFolderName);
                currentFolder.Folders.Add(parentFolder, folder);
                _contents.Add(folder);
            }

            currentFolder = folder;

            parentFolderSequence.Add(currentFolder);
        }
        
        sb.Append(TarPathSeparator).Append(file.Name);

        var relativePathAndFileName = sb.ToString();
        Log.Debug("Adding file entry {FileName} with path {RelativePathAndName}", 
            file.Name, 
            relativePathAndFileName);

        var tarItemFile = new TarItemFile(file, file.Name, parentFolderSequence, sb.ToString())
        {
            ModificationTime = DateTimeOffset.FromFileTime(file.LastWriteTime.ToFileTimeUtc())
        };
        
        currentFolder.Files.Add(file.Name, tarItemFile);
        _contents.Add(tarItemFile);

        return tarItemFile;
    }

    public bool GetFolder(string relativeFolderPath, bool createIfNotExists, [NotNullWhen(true)] out TarItemFolder? tarItemFolder) 
    {
        if (createIfNotExists)
            Log.Debug("Getting the folder {TheFolderPath} creating any missing folders along the way", relativeFolderPath);
        else 
            Log.Debug("Looking for the folder {TheFolderPath}", relativeFolderPath);
        
        var sb = new StringBuilder(relativeFolderPath.Length + _additionalRootFolder.Length + 10);
        var parentFolderSequence = new List<TarItemFolder>();
        var currentFolder = _anchor;
        var first = true;
        
        foreach (var parentFolder in BuildTarballPathFolderSequenceFromRelativeFolderPath(relativeFolderPath))
        {
            if (first)
                first = false;
            else
                sb.Append(TarPathSeparator);

            sb.Append(parentFolder);

            if (!currentFolder.Folders.TryGetValue(parentFolder, out var folder))
            {
                var relativePathAndFolderName = sb.ToString();

                if (!createIfNotExists)
                {
                    Log.Debug("Could not find {TheFolderPath} because {RelativePathAndFolderName} does not exist", 
                        relativeFolderPath, 
                        relativePathAndFolderName);
                    tarItemFolder = null;
                    return false;
                }
                
                Log.Debug("Adding folder entry {FolderName} with path {RelativePathAndName}", 
                    parentFolder, 
                    relativePathAndFolderName);
                folder = new TarItemFolder(parentFolder, parentFolderSequence, relativePathAndFolderName);
                currentFolder.Folders.Add(parentFolder, folder);
                _contents.Add(folder);
            }

            currentFolder = folder;

            parentFolderSequence.Add(currentFolder);
        }

        tarItemFolder = currentFolder;
        return true;
    }
    
    public bool Save(FileInfo archiveFile)
    {
        archiveFile.Refresh();
        if (archiveFile.Exists)
            archiveFile.Delete();

        using var fileStream = archiveFile.OpenWrite();
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
        using var tarWriter = new TarWriter(gzipStream);
        
        foreach (var content in OrderedAndFilteredContents)
        {
            content.SaveToTarball(tarWriter);
        }

        return true;
    }

    public IEnumerable<TarItem> OrderedAndFilteredContents => _contents
        .Where(ti => !ti.AncestorOrItemIsHidden)
        .OrderBy(ti => ti.RelativePathAndName);

    private List<string> BuildTarballPathFolderSequenceFromRelativeFolderPath(string relativeFolderPath)
    {
        var retval = new List<string>();
        
        if (!string.IsNullOrEmpty(_additionalRootFolder))
            retval.Add(_additionalRootFolder);
        
        var names = relativeFolderPath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
        
        var first = (0 < names.Length && names[0] == ".") ? 1 : 0;
            
        for (var i = first; i < names.Length; i++)
        {
            retval.Add(names[i]);    
        }

        return retval;
    }
    
    private List<string> BuildTarballPathFolderSequence(string? rootedPath)
    {
        if (rootedPath is null ||
            !rootedPath.StartsWith(_basePath, StringComparison.Ordinal))
        {
            Log.Error("The path {RootedPath} lies outside the base path {TheBasePath}",
                rootedPath,
                _basePath);
            throw new ArgumentException("The path lies outside the base path", nameof(rootedPath));
        }

        var retval = new List<string>();
        
        if (!string.IsNullOrEmpty(_additionalRootFolder))
            retval.Add(_additionalRootFolder);
        
        var relativePath = Path.GetRelativePath(_basePath, rootedPath);
        var names = relativePath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
        
        var first = (0 < names.Length && names[0] == ".") ? 1 : 0;
            
        for (var i = first; i < names.Length; i++)
        {
            retval.Add(names[i]);    
        }

        return retval;
    }
    
    private readonly List<TarItem> _contents = [];
    private readonly TarItemFolder _anchor = new TarItemFolder("anchor", Array.Empty<TarItemFolder>(), string.Empty);
    private readonly string _basePath  = Guard.Against.InvalidInput(basePath, nameof(basePath), s => !string.IsNullOrWhiteSpace(s) && Directory.Exists(s));
    private readonly string _additionalRootFolder = string.IsNullOrWhiteSpace(additionalRootFolder) 
        ? string.Empty 
        : additionalRootFolder;
    private static readonly char[] PathSeparators = ['\\', '/'];
    private static readonly char TarPathSeparator = '/';
}
