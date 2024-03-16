using System.Text;

namespace vigobase;

internal class EnvironmentDescriptor(string name) : IEnvironmentDescriptor
{
    public NamedTag EnvironmentName { get; } = new(name);
    public IEnumerable<NamedTag> Aliases => EnvironmentAliases;
    public IEnumerable<NamedTag> Groups => EnvironmentGroups;
    
    public bool HasTag(NamedTag tag)
    {
        return EnvironmentName == tag ||
               EnvironmentAliases.Any(t => t == tag) ||
               EnvironmentGroups.Any(t => t == tag);
    }

    public bool HasAnyOfTheseTags(IEnumerable<NamedTag> tags)
    {
        return tags.Any(HasTag);
    }
    
    public bool HasNoneOfTheseTags(IEnumerable<NamedTag> tags)
    {
        return tags.All(t => !HasTag(t));
    }

    public override string ToString()
    {
        var sb = new StringBuilder($"[Environment: {EnvironmentName.Name}");
        
        if (0 < EnvironmentAliases.Count)
        {
            sb.Append(", Aliases: {")
              .Append(string.Join(',', EnvironmentAliases))
              .Append('}');
        }
        
        if (0 < EnvironmentGroups.Count)
        {
            sb.Append(", Groups: {")
              .Append(string.Join(',', EnvironmentGroups))
              .Append('}');
        }

        sb.Append(']');

        return sb.ToString();
    }

    internal readonly List<NamedTag> EnvironmentAliases = [];
    internal readonly List<NamedTag> EnvironmentGroups = [];
}