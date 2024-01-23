namespace vigobase;

public interface IEnvironmentDescriptor
{
    NamedTag EnvironmentName { get; }
    IEnumerable<NamedTag> Aliases { get; }
    IEnumerable<NamedTag> Groups { get; }
    bool HasTag(NamedTag tag);
    bool HasAnyOfTheseTags(IEnumerable<NamedTag> tags);
    bool HasNoneOfTheseTags(IEnumerable<NamedTag> tags);
    string ToString();
}