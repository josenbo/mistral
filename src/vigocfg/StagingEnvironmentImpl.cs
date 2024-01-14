namespace vigocfg;

internal class StagingEnvironmentImpl : IStagingEnvironment
{
    public string Key => EnvironmentHelper.Staging.Key();
    public string Name => EnvironmentHelper.Staging.Name();
}