using JetBrains.Annotations;

namespace vigoconfig;

[PublicAPI]
public static class FolderConfigurationApi
{
    public static IConfigurationReader Reader => _reader ??= new ConfigurationReader();

    private static IConfigurationReader? _reader;
}