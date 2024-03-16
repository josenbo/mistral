using JetBrains.Annotations;

namespace vigorule;

[PublicAPI]
public interface IMutableHandling
{
    bool CanDeploy { get; set; } 
}