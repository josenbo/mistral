using System.Text;
using vigobase;

namespace vigorule;

internal class DirectoryHandlingImpl
(
    DirectoryInfo sourceDirectory, 
    bool keepEmptyDirectory,
    bool isTopLevelDirectory,
    Func<IEnumerable<string>> enumTargets
)
: IMutableDirectoryHandling, IFinalDirectoryHandling
{
    public DirectoryInfo SourceDirectory { get; } = sourceDirectory;
    public bool KeepEmptyDirectory { get; } = keepEmptyDirectory;
    public bool IsTopLevelDirectory { get; } = isTopLevelDirectory;
    public bool CanDeploy { get; set; }
    public IEnumerable<string> DeploymentTargets => _targets;
    public bool CheckedSuccessfully => true;

    
    public bool HasDeploymentTarget(string target)
    {
        return _targets.Contains(target, StringComparer.InvariantCultureIgnoreCase);
    }

    public bool CanDeployForTarget(string target)
    {
        return !IsTopLevelDirectory && (KeepEmptyDirectory || HasDeploymentTarget(target));
    }

    public void Explain(StringBuilder sb, ExplainSettings settings)
    {
        // todo: add implementation
        sb.Append(nameof(DirectoryHandlingImpl))
            .Append('.')
            .Append(nameof(Explain))
            .AppendLine(" still needs to be implemented");
    }
    
    IFinalDirectoryHandling IMutableDirectoryHandling.CheckAndTransform()
    {
        _targets.Clear();
        _targets.AddRange(enumTargets());
        return this;
    }

    private readonly List<string> _targets = [];
}