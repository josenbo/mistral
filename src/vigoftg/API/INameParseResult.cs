using JetBrains.Annotations;
using vigobase;

namespace vigoftg;

[PublicAPI]
public interface INameParseResult
{
    bool CanDeploy { get; }
    string SourceName { get; }
    string TargetName { get; }
    IEnumerable<NamedTag> Tags { get; }
}