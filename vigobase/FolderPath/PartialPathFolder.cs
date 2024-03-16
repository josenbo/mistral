using System.Text.RegularExpressions;
using Ardalis.GuardClauses;

namespace vigobase;

public partial class PartialPathFolder(string name)
{
    public string Name
    {
        get => _name;
        set => _name = Guard.Against.InvalidInput(value, nameof(value), IsValidFolderName);
    } 

    public FilePermission Permission { get; set; } = FilePermission.UseDefault;
    public bool CreateIfMissing { get; set; } = true;
    
    private static bool IsValidFolderName(string name)
    {
        return !string.IsNullOrEmpty(name) && RexValidFolderName.IsMatch(name);
    }

    private static readonly Regex RexValidFolderName = MyRegex();
    private string _name = Guard.Against.InvalidInput(name, nameof(name), IsValidFolderName);

    [GeneratedRegex(@"^[a-zA-Z0-9]([-_ a-zA-Z0-9]{0,38}[a-zA-Z0-9])?$", RegexOptions.None)]
    private static partial Regex MyRegex();
}