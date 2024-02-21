using JetBrains.Annotations;
using vigobase;

namespace vigoconfig;

[PublicAPI]
public interface IFolderConfiguration
{
    bool KeepEmptyFolder { get; }
    FileHandlingParameters InitialDefaults { get; }
    FileHandlingParameters FolderDefaults { get; }
    IEnumerable<IFileRuleConfiguration> RuleConfigurations { get; }
}