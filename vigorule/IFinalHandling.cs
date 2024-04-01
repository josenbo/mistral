using System.Text;
using JetBrains.Annotations;

namespace vigorule;

[PublicAPI]
public interface IFinalHandling
{
    bool CheckedSuccessfully { get; }
    bool CanDeploy { get; }
    IEnumerable<string> DeploymentTargets { get; }
    bool HasDeploymentTarget(string target);
    bool CanDeployForTarget(string target);
    void Explain(StringBuilder sb, ExplainSettings settings);
}