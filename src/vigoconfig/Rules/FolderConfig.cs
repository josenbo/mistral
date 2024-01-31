namespace vigoconfig;

internal class FolderConfig
{
    public bool KeepEmptyFolder { get; set; } = false;
    public bool DefaultActionSkip { get; set; } = false;

    public IEnumerable<Rule> Rules => _rules;

    public int NextIndex => _rules.Count;
    public void AddRule(Rule rule)
    {
        _rules.Add(rule);
    }
    
    private readonly List<Rule> _rules = [];
}