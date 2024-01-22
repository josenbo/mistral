using JetBrains.Annotations;

namespace vigoftg;

[PublicAPI]
public interface INameParseResult
{
    bool CanDeploy { get; }
    string SourceName { get; }
    string TargetName { get; }
    IEnumerable<string> Tags { get; }
    bool HasTag(string tag);
}