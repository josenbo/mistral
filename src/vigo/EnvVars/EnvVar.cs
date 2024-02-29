namespace vigo;

internal abstract class EnvVar
{
    public abstract string? GetEnvironmentVariable(string environmentVariableName);
    public abstract string GetEnvironmentVariable(string environmentVariableName, string defaultValue);

    public static EnvVarSystem GetSystem() => EnvVarSystemInstance;
    public static EnvVarMock GetMock() => new EnvVarMock();
    
    private static readonly EnvVarSystem EnvVarSystemInstance = new EnvVarSystem();
}