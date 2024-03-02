using System.Text;
using System.Text.RegularExpressions;
using vigobase;

namespace vigorule;

internal record RepositoryReadRequest(
    DirectoryInfo TopLevelDirectory,
    IReadOnlyList<ConfigurationFilename> ConfigFiles,
    bool WalkFolderTree,
    FileHandlingParameters DefaultHandling
)
{
    public string GetTopLevelRelativePath(string path)
    {
        return Path.GetRelativePath(TopLevelDirectory.FullName, path);
    }
    
    public string GetTopLevelRelativePath(FileSystemInfo fileSystemItem)
    {
        return Path.GetRelativePath(TopLevelDirectory.FullName, fileSystemItem.FullName);
    }

    public string GetConfigFilesRegexPattern()
    {
        var sb = new StringBuilder();
        var first = true;

        foreach (var name in ConfigFiles.Select(cf => cf.FileName))
        {
            if (first)
                first = false;
            else 
                sb.Append('|');

            sb.Append($"(^{Regex.Escape(name)}$)");
        }

        return sb.ToString();
    }

    public FileHandlingParameters IgnoredFileHandling => _ignoredFileHandling ??= DefaultHandling with 
        { FileType = FileTypeEnum.BinaryFile, Permissions = FilePermission.UseDefault };
    
    private FileHandlingParameters? _ignoredFileHandling;
}