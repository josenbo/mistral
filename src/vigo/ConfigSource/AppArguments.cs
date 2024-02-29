using vigobase;

namespace vigo;

internal record AppArguments(
    CommandEnum? Command,
    bool? ShowHelp,
    bool? ShowVersion,
    CommandEnum? CommandToShowHelpFor,
    DirectoryInfo? RepositoryRoot,
    FileInfo? DeploymentBundle,
    IReadOnlyList<string>? Targets,
    FileInfo? ConfigurationFile,
    IReadOnlyList<string>? Names
)
{
    public static AppArguments Empty { get; } = new AppArguments(
        null, 
        null, 
        null, 
        null,
        null,
        null,
        null,
        null,
        null);
}