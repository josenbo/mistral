using System.Text;

namespace vigoarchive;

public record TarItemFile(
    string TarRelativePath,
    UnixFileMode FileMode,
    DateTimeOffset ModificationTime,
    FileInfo TransformedContent
) : TarItem(
    TarRelativePath,
    FileMode,
    ModificationTime
)
{
    internal override IEnumerable<string> FolderPaths
    {
        get
        {
            var sb = new StringBuilder("", TarRelativePath.Length);
            
            for (var i = 0; i < ItemArray.Length - 1; i++)
            {
                if (0 < i)
                    sb.Append(TarPathSeparator);
                sb.Append(ItemArray[i]);
                
                yield return sb.ToString();
            }            
        }
    }
}