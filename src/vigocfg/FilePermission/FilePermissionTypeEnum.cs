using JetBrains.Annotations;

namespace vigocfg;

/// <summary>
/// Tell if and how a file permission is specified
/// </summary>
[PublicAPI]
public enum FilePermissionTypeEnum
{
    /// <summary>
    /// Only use this value to check, if a variable lacks proper initialization
    /// </summary>
    Undefined = 0,
    
    /// <summary>
    /// Do not override the system-defined file permission default 
    /// </summary>
    Default = 114001,
    
    /// <summary>
    /// The desired file permission is given in octal notation (e.g. 664)
    /// </summary>
    Octal = 114002,
    
    /// <summary>
    /// The desired file permission is given in symbolic notation (e.g. u+x)
    /// </summary>
    Symbolic = 114003,
}
