using System.Text;
using JetBrains.Annotations;

namespace vigorule;

[PublicAPI]
public interface IFinalHandling
{
    bool CheckedSuccessfully { get; }
    bool CanDeploy { get; }

    void Explain(StringBuilder sb, ExplainSettings settings);
}