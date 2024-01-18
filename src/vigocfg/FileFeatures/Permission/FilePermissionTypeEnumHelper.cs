using JetBrains.Annotations;

namespace vigocfg;

/// <summary>
/// Provide extension methods and static helper methods for the file permission type
/// </summary>
/// <see cref="FilePermissionTypeEnum"/>
[PublicAPI]
public static class FilePermissionTypeEnumHelper
{
    /// <summary>
    /// Take an integer value and check if it represents an existing file permission enum 
    /// </summary>
    /// <param name="value">The integer value to check</param>
    /// <example>
    /// This sample code will return false, because 4711 is not a valid file permission type enum
    /// <code>
    /// var ok = FilePermissionTypeEnumHelper.IsDefined(4711);
    /// </code>
    /// </example>
    /// <returns>True, if an enum with the given value exists</returns>
    public static bool IsDefined(int value) => Enum.IsDefined(typeof(FilePermissionTypeEnum), value);
    
    /// <summary>
    /// Extension method to check if the file permission enum value is in the set of defined enums
    /// </summary>
    /// <param name="value">The (file permission enum) object that the extension method is called upon</param>
    /// <example>
    /// This sample code will return false, because 4711 is not a valid file permission type enum
    /// <code>
    /// var invalidEnum = (FilePermissionTypeEnum)4711;
    /// var ok = invalidEnum.IsDefined();
    /// </code>
    /// </example>
    /// <returns>True, if an enum with the given value exists</returns>
    public static bool IsDefined(this FilePermissionTypeEnum value) => Enum.IsDefined(typeof(FilePermissionTypeEnum), value);

    /// <summary>
    /// Take an integer value and check if it represents an
    /// existing file permission and differs from zero.
    /// A zero value reveals, that the variable has not been
    /// properly initialized.
    /// </summary>
    /// <param name="value">The integer value to check</param>
    /// <example>
    /// This sample code will return false, because although
    /// zero is a valid file permission type enum, it signals
    /// that initialization did not take place
    /// <code>
    /// var ok = FilePermissionTypeEnumHelper.IsDefinedAndValid(0);
    /// </code>
    /// </example>
    /// <returns>True, if an enum with the given value exists and differs from zero</returns>
    public static bool IsDefinedAndValid(int value) => Enum.IsDefined(typeof(FilePermissionTypeEnum), value) && (FilePermissionTypeEnum)value != FilePermissionTypeEnum.Undefined;

    /// <summary>
    /// Extension method to check if the file permission enum value
    /// is in the set of defined enums and differs from zero.
    /// A zero value reveals, that the variable has not been
    /// properly initialized.
    /// </summary>
    /// <param name="value">The (file permission enum) object that the extension method is called upon</param>
    /// <example>
    /// This sample code will return false, because although
    /// zero is a valid file permission type enum, it signals
    /// that initialization did not take place
    /// <code>
    /// var invalidEnum = (FilePermissionTypeEnum)0;
    /// var ok = invalidEnum.IsDefinedAndValid();
    /// </code>
    /// </example>
    /// <returns>True, if an enum with the given value exists and differs from zero</returns>
    public static bool IsDefinedAndValid(this FilePermissionTypeEnum value) => Enum.IsDefined(typeof(FilePermissionTypeEnum), value) && value != FilePermissionTypeEnum.Undefined;
    
    /// <summary>
    /// Extension method to check if a file permission was specified using octal notation (e.g. 664) 
    /// </summary>
    /// <param name="value">The (file permission enum) object that the extension method is called upon</param>
    /// <example>
    /// This sample code will return true
    /// <code>
    /// var perm = FilePermissionTypeEnum.Octal;
    /// var ok = perm.IsOctalNotation();
    /// </code>
    /// </example>
    /// <returns>True, if the file permission is specified using octal notation</returns>
    public static bool IsOctalNotation(this FilePermissionTypeEnum value) => value == FilePermissionTypeEnum.Octal;
    
    /// <summary>
    /// Extension method to check if a file permission was specified using symbolic notation (e.g. u+x) 
    /// </summary>
    /// <param name="value">The (file permission enum) object that the extension method is called upon</param>
    /// <example>
    /// This sample code will return true
    /// <code>
    /// var perm = FilePermissionTypeEnum.Symbolic;
    /// var ok = perm.IsSymbolicNotation();
    /// </code>
    /// </example>
    /// <returns>True, if the file permission is specified using symbolic notation</returns>
    public static bool IsSymbolicNotation(this FilePermissionTypeEnum value) => value == FilePermissionTypeEnum.Symbolic;
}