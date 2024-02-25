using System.Diagnostics.CodeAnalysis;
using Serilog;
using vigobase;

namespace vigoconfig;

internal class ConfigurationReader : IConfigurationReader
{
    public bool TryParse(
        string configurationScript, 
        string configurationFile, 
        ConfigurationFileTypeEnum configurationType, 
        FileHandlingParameters initialDefaults, 
        [NotNullWhen(true)] out IFolderConfiguration? folderConfiguration)
    {

        try
        {
            var partialFolderConfig = PartialFolderConfigReader.Parse(configurationScript, configurationFile, configurationType);

            folderConfiguration = new FolderConfiguration(initialDefaults, partialFolderConfig);

            return true;
        }
        catch (VigoException e)
        {
            Log.Error(e, "Parsing the configuration failed");
            folderConfiguration = null;
            return false;
        }
    }
}