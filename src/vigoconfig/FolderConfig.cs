using JetBrains.Annotations;

namespace vigoconfig;

[PublicAPI]
public class FolderConfig
{
    public bool? KeepEmptyFolder { get; set; }
    public FolderConfigPartialHandling? LocalDefaults { get; set; }
    public IList<FolderConfigPartialRule> PartialRules { get; } = [];
}