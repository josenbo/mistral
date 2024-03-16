using System.Collections;

namespace vigobase;

public class PartialPath : IReadOnlyList<PartialPathFolder>
{
    public PartialPath(params string[] folders)
    {
        foreach (var folder in folders)
        {
            _folders.Add(new PartialPathFolder(folder));
        }
    }
    
    public IEnumerator<PartialPathFolder> GetEnumerator()
    {
        return _folders.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _folders.GetEnumerator();
    }

    public int Count => _folders.Count;

    public PartialPathFolder this[int index] => _folders[index];

    private readonly List<PartialPathFolder> _folders = [];
}