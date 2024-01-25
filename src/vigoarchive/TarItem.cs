using System.Formats.Tar;
using System.Text;
using Ardalis.GuardClauses;
using vigobase;

namespace vigoarchive;

public abstract class TarItem
{
    protected internal TarItem(string name, IEnumerable<TarItemFolder> parentFolderSequence, string relativePathAndName)
    {
        Name = Guard.Against.NullOrWhiteSpace(name);
        _parentFolderSequence = parentFolderSequence.ToList();
        RelativePathAndName = relativePathAndName;
    }

    public string Name { get; }
    public string RelativePathAndName { get; }
    public IReadOnlyList<TarItemFolder> ParentFolderSequence => _parentFolderSequence;
    public FilePermission Permissions { get; set; } = FilePermission.UseDefault;
    public DateTimeOffset ModificationTime { get; set; } = DateTimeOffset.Now;

    internal virtual bool AncestorOrItemIsHidden { get; } = false;
    internal abstract void SaveToTarball(TarWriter tarWriter);
    
    private readonly List<TarItemFolder> _parentFolderSequence;
}