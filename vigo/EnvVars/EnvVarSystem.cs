using System.Collections;
using Ardalis.GuardClauses;

namespace vigo;

internal class EnvVarSystem : EnvVar
{
    public override string? GetEnvironmentVariable(string environmentVariableName)
    {
        return Environment.GetEnvironmentVariable(environmentVariableName);
    }

    public override string GetEnvironmentVariable(string environmentVariableName, string defaultValue)
    {
        Guard.Against.NullOrWhiteSpace(defaultValue, nameof(defaultValue));
        var envValue = Environment.GetEnvironmentVariable(environmentVariableName);
        return string.IsNullOrWhiteSpace(envValue) ? defaultValue : envValue;
    }

    public override IEnumerable<string> GetEnvironmentVariables()
    {
        var envVarDict = Environment.GetEnvironmentVariables();

        foreach (var entryKey in from DictionaryEntry kv in envVarDict select kv.Key)
        {
            if (entryKey is string entryKeyAsString)
                yield return entryKeyAsString;
        }
    }
}