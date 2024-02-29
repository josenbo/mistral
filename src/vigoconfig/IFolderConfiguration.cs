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
    IConfigurationScriptExtract? BasedOn { get; }

    /// <summary>
    /// The configuration of rules should only
    /// specify the noteworthy and rely on defaults
    /// for the rest. To this end the global default
    /// can be refined in folders before being
    /// modified by the rule. This member lists the
    /// defaults that were successively applied in
    /// descending order with the global defaults
    /// coming last.
    /// </summary>
    /// <returns></returns>
    IEnumerable<FileHandlingParameters> GetHandlingDefaultsChain();
}