using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using vigobase;

namespace vigoconfig;

[PublicAPI]
public interface IConfigurationReader
{
    bool TryParse(string configationScript, FileHandlingParameters initialDefaults, [NotNullWhen(true)] out IFolderConfiguration folderConfiguration);
}