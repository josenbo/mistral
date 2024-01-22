namespace vigobase;

public class EnvironmentDescriptorBuilder(string name)
{
    public EnvironmentDescriptorBuilder AddAlias(string alias)
    {
        if (_isBuild)
            throw new InvalidOperationException(
                $"Cannot add alias tag to {_desc.EnvironmentName} because the {nameof(IEnvironmentDescriptor)} instance has already been build");
        
        _desc.EnvironmentAliases.Add(new NamedTag(alias));
        return this;
    }
    public EnvironmentDescriptorBuilder AddGroup(string group)
    {
        if (_isBuild)
            throw new InvalidOperationException(
                $"Cannot add group tag to {_desc.EnvironmentName} because the {nameof(IEnvironmentDescriptor)} instance has already been build");
        
        _desc.EnvironmentGroups.Add(new NamedTag(group));
        return this;
    }
    
    public EnvironmentDescriptorBuilder AddAliases(IEnumerable<string> aliases)
    {
        foreach (var alias in aliases)
        {
            _desc.EnvironmentAliases.Add(new NamedTag(alias));
        }
        return this;
    }
    
    public EnvironmentDescriptorBuilder AddGroups(IEnumerable<string> groups)
    {
        foreach (var group in groups)
        {
            _desc.EnvironmentGroups.Add(new NamedTag(group));
        }
        return this;
    }

    public IEnvironmentDescriptor Build()
    {
        _isBuild = true;
        return _desc;
    }

    private bool _isBuild = false;
    private readonly EnvironmentDescriptor _desc = new(name);
}