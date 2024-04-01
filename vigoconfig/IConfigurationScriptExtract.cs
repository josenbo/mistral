using JetBrains.Annotations;

namespace vigoconfig;

/// <summary>
/// Information about the configuration script section
/// on which folder or rule settings are based
/// </summary>
[PublicAPI]
public interface IConfigurationScriptExtract
{
    /// <summary>
    /// The sequence number of the configuration section
    /// in the configuration script. The first section in
    /// the script gets the number 1.
    /// </summary>
    int Position { get; }
    
    /// <summary>
    /// A one-liner identifying the configuration section.
    /// Its purpose is to be displayed in console messages
    /// or logs showing why a file is handled the way it is
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// The name of the configuration file or another
    /// description of the configurations origin.
    /// A relative file path in the repository is the
    /// intended use, but it can be any text that is
    /// not too long.  
    /// </summary>
    string ConfigurationFile { get; }
    
    /// <summary>
    /// The line number in the configuration script
    /// where the section starts
    /// </summary>
    int FromLineNumber { get; }

    /// <summary>
    /// The line number in the configuration script
    /// where the section nds
    /// </summary>
    int ToLineNumber { get; }
    
    /// <summary>
    /// The text of the configuration section without
    /// comments or empty lines. The lines are terminated
    /// with single newline characters (unix style)
    /// </summary>
    string Content { get; }
}