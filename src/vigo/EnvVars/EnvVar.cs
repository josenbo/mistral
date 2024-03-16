namespace vigo;

internal abstract class EnvVar
{
    public abstract string? GetEnvironmentVariable(string environmentVariableName);
    public abstract string GetEnvironmentVariable(string environmentVariableName, string defaultValue);
    public abstract IEnumerable<string> GetEnvironmentVariables();

    public static EnvVarSystem GetSystem() => EnvVarSystemInstance;
    public static EnvVarMock GetMock() => new EnvVarMock(false);
    public static EnvVarMock GetMock(bool includeSystemEnvironmen) => new EnvVarMock(includeSystemEnvironmen);
    
    private static readonly EnvVarSystem EnvVarSystemInstance = new EnvVarSystem();
}