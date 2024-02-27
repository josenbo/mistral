namespace vigo;

internal class JobRunnerDoNothing(AppConfigRepo configRepo)
{
    public bool Success { get; } = true;
    public bool Prepare() => true;
    public bool Run() => true;

    public void CleanUp()
    {
    }
}