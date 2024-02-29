namespace vigo;

internal class JobRunnerDoNothing(AppConfig appConfig) : IJobRunner
{
    private AppConfig AppConfig { get; } = appConfig;
    public bool Success { get; } = true;
    public bool Prepare() => true;
    public bool Run() => true;

    public void CleanUp()
    {
    }
}