using JetBrains.Annotations;
using vigobase;

namespace vigoconfig;

/// <summary>
/// A container for file rules, folder-specific settings
/// and a record of the default values used in building
/// the file rules
/// </summary>
[PublicAPI]
public interface IFolderConfiguration
{
    /// <summary>
    /// The list of rules in found in the configuration file
    /// </summary>
    IEnumerable<IFileRuleConfiguration> RuleConfigurations { get; }

    /// <summary>
    /// If set, the folder will be contained in the
    /// deployment bundle even if it does not contain
    /// any files
    /// </summary>
    bool KeepEmptyFolder { get; }
    
    /// <summary>
    /// The default values for file handling passed in
    /// when building th folder configuration
    /// </summary>
    FileHandlingParameters InitialDefaults { get; }
    
    /// <summary>
    /// The initial values with the folder-specific
    /// defaults added. These are the defaults used
    /// for building the file rules 
    /// </summary>
    FileHandlingParameters FolderDefaults { get; }
    
    /// <summary>
    /// If there was a folder-specific section in
    /// the configuration file, its content can
    /// be accessed here.
    /// </summary>
    string? BasedOnTheConfigurationText { get; }
}