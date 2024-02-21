using JetBrains.Annotations;

namespace vigoconfig;

/// <summary>
/// The static entry point for the libraries services 
/// </summary>
[PublicAPI]
public static class FolderConfigurationApi
{
    /// <summary>
    /// Access to services for reading deployment configuration
    /// files and building rulesets for file handling 
    /// </summary>
    public static IConfigurationReader Reader => _reader ??= new ConfigurationReader();

    private static IConfigurationReader? _reader;
}