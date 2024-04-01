using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using Serilog;

namespace vigobase;

public partial record NamedTag(string Name) : IComparable<NamedTag>, IComparable
{
    public string Name { get; } = Guard.Against.InvalidInput<string>(Name, nameof(Name), IsValidTagName);

    public virtual bool Equals(NamedTag? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
    }

    public override int GetHashCode()
    {
        return StringComparer.InvariantCultureIgnoreCase.GetHashCode(Name);
    }

    public override string ToString()
    {
        return Name.ToString();
    }

    public int CompareTo(NamedTag? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return string.Compare(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
    }

    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(null, obj)) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        return obj is NamedTag other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(NamedTag)}");
    }

    public static bool operator <(NamedTag? left, NamedTag? right)
    {
        return Comparer<NamedTag>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(NamedTag? left, NamedTag? right)
    {
        return Comparer<NamedTag>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(NamedTag? left, NamedTag? right)
    {
        return Comparer<NamedTag>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(NamedTag? left, NamedTag? right)
    {
        return Comparer<NamedTag>.Default.Compare(left, right) >= 0;
    }

    private static bool IsValidTagName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Log.Error("The tag name must not be empty");
            return false;
        }
        
        switch (name.Length)
        {
            case < 3:
                Log.Error("The tag name \"{TagName}\" is to short. Valid tag names must have at least 3 characters", name);
                return false;
            case > 40:
                Log.Error("The tag name \"{TagName}\" is to long. Valid tag names must have no more than 40 characters", name[..40] + "..");
                return false;
            default:
                if (RexTagName.IsMatch(name)) return true;
                
                Log.Error("The tag name \"{TagName}\" violates the syntax rules which require letters followed by digits or letters optionally separated by single - (dash), _ (underscrore) or . (dot) characters", name);
                return false;
        }
    }
    
    private static readonly Regex RexTagName = MyRegex();

    [GeneratedRegex(@"^[a-zA-Z]([-_.]?[a-zA-Z0-9]{1,100}){1,100}$", RegexOptions.None)]
    private static partial Regex MyRegex();

}