using System.Text;

namespace vigoarchive;

public record TarItemDirectory(
    string TarRelativePath,
    UnixFileMode FileMode,
    DateTimeOffset ModificationTime
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
            
            for (var i = 0; i < ItemArray.Length; i++)
            {
                if (0 < i)
                    sb.Append(TarPathSeparator);
                sb.Append(ItemArray[i]);
                
                yield return sb.ToString();
            }            
        }
    }
}