using Ardalis.GuardClauses;

namespace vigo;

internal class EnvVarMock(bool includeSystemEnvironmen) : EnvVar
{
    public EnvVarMock Add(string name, string value)
    {
        _dict.TryAdd(name, value);
        return this;
    }

    public override string? GetEnvironmentVariable(string environmentVariableName)
    {
        if (!includeSystemEnvironmen) 
            return _dict.GetValueOrDefault(environmentVariableName);
        
        return _dict.TryGetValue(environmentVariableName, out var value) 
            ? value 
            : Environment.GetEnvironmentVariable(environmentVariableName);
    }

    public override string GetEnvironmentVariable(string environmentVariableName, string defaultValue)
    {
        Guard.Against.NullOrWhiteSpace(defaultValue, nameof(defaultValue));

        if (!includeSystemEnvironmen) 
            return _dict.GetValueOrDefault(environmentVariableName, defaultValue);
        
        if (_dict.TryGetValue(environmentVariableName, out var value))
            return value;
        var envValue = Environment.GetEnvironmentVariable(environmentVariableName);
        return string.IsNullOrWhiteSpace(envValue) ? defaultValue : envValue;
    }

    public override IEnumerable<string> GetEnvironmentVariables()
    {
        foreach (var key in _dict.Keys)
        {
            yield return key;
        }
        
        if (!includeSystemEnvironmen)
            yield break;

        foreach (var key in (IEnumerable<string>)Environment.GetEnvironmentVariables().Keys)
        {
            if (!_dict.ContainsKey(key))
                yield return key;
        }
    }

    private readonly Dictionary<string, string> _dict = new Dictionary<string, string>(StringComparer.Ordinal);
}