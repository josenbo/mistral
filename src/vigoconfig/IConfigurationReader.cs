using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace vigoconfig;

[PublicAPI]
public interface IConfigurationReader
{
    bool TryParse(string configationScript, [NotNullWhen(true)] out IFolderConfiguration folderConfiguration);
}