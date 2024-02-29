using Ardalis.GuardClauses;

namespace vigo;

internal class EnvVarMock : EnvVar
{
    public EnvVarMock Add(string name, string value)
    {
        _dict.TryAdd(name, value);
        return this;
    }

    public override string? GetEnvironmentVariable(string environmentVariableName)
    {
        return _dict.GetValueOrDefault(environmentVariableName);
    }

    public override string GetEnvironmentVariable(string environmentVariableName, string defaultValue)
    {
        Guard.Against.NullOrWhiteSpace(defaultValue, nameof(defaultValue));
        return _dict.GetValueOrDefault(environmentVariableName, defaultValue);
    }

    private readonly Dictionary<string, string> _dict = new Dictionary<string, string>(StringComparer.Ordinal);
}