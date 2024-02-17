namespace vigoconfig;

public class FolderConfig
{
    public bool? KeepEmptyFolder { get; set; }
    public PartialFileHandlingParameters? LocalDefaults { get; set; }
    public IList<PartialFileRule> PartialRules { get; } = [];
}