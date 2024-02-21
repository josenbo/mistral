using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal class ConfigurationReader : IConfigurationReader
{
    public bool TryParse(
        string configationScript, 
        FileHandlingParameters initialDefaults, 
        [NotNullWhen(true)] out IFolderConfiguration folderConfiguration)
    {
        throw new NotImplementedException();
    }
}