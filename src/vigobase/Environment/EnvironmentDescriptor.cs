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

    public bool HasAnyTag(IEnumerable<NamedTag> tags)
    {
        return tags.Any(HasTag);
    }

    internal readonly List<NamedTag> EnvironmentAliases = [];
    internal readonly List<NamedTag> EnvironmentGroups = [];
}