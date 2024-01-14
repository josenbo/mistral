using JetBrains.Annotations;

namespace vigoftg;

[PublicAPI]
public interface INameParseResult
{
    bool Success { get; }
    bool DoIgnore { get; }
    bool DoRename { get; }
    string CurrentName { get; }
    string NewName { get; }
    IEnumerable<string> Tags { get; }
    bool HasTag(string tag);
}