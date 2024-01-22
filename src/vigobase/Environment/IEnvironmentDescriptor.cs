namespace vigobase;

public interface IEnvironmentDescriptor
{
    NamedTag EnvironmentName { get; }
    IEnumerable<NamedTag> Aliases { get; }
    IEnumerable<NamedTag> Groups { get; }
    bool HasTag(NamedTag tag);
    bool HasAnyTag(IEnumerable<NamedTag> tags);
}