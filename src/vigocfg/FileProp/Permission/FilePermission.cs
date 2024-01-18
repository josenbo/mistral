using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace vigocfg;

/// <summary>
/// Specification of desired file permissions.
/// Currently, only linux-style file permissions
/// are supported.
/// </summary>
[PublicAPI]
public abstract partial record FilePermission
{
    /// <summary>
    /// Tell if and how the file permission is specified
    /// </summary>
    public abstract FilePermissionTypeEnum FilePermissionType { get; }
    
    /// <summary>
    /// Static factory method for using the default file permission
    /// </summary>
    public static FilePermissionDefault UseDefault => new FilePermissionDefault();

    /// <summary>
    /// Static factory method for a file permission that is specified using octal notation
    /// </summary>
    /// <param name="octalDigits">A string with three octal digits</param>
    /// <example>
    /// Example usage:
    /// <code>
    /// var octalPerm = FilePermission.SpecifyOctal("644");
    /// </code>
    /// This is equivalent to:
    /// <code>
    /// var octalPerm = new FilePermissionOctal("644");
    /// </code>
    /// </example>
    /// <exception cref="ArgumentException">If the parameter is not recognized as a valid octal notation</exception>
    /// <returns>A FilePermissionOctal object with the given octal value</returns>
    public static FilePermissionOctal SpecifyOctal(string octalDigits)
    {
        // Guard.Against.InvalidInput(octalDigits, nameof(octalDigits), IsValidOctalNotation);

        return new FilePermissionOctal(octalDigits);
    }

    /// <summary>
    /// Static factory method for a file permission that is specified using symbolic notation
    /// </summary>
    /// <param name="symbolicNotation"></param>
    /// <example>
    /// Example usage:
    /// <code>
    /// var symbolicPerm = FilePermission.SpecifySymbolic("u+x");
    /// </code>
    /// This is equivalent to:
    /// <code>
    /// var symbolicPerm = new FilePermissionSymbolic("u+x");
    /// </code>
    /// </example>
    /// <exception cref="ArgumentException">If the parameter is not recognized as a valid symbolc notation</exception>
    /// <returns>A FilePermissionSymbolic object with the given symbolic value</returns>
    public static FilePermissionSymbolic SpecifySymbolic(string symbolicNotation)
    {
        // Guard.Against.InvalidInput(symbolicNotation, nameof(symbolicNotation), IsValidSymbolicNotation);

        return new FilePermissionSymbolic(symbolicNotation);
    }

    internal static bool IsValidOctalNotation(string textRepresentation) => RexOctalDigits.IsMatch(textRepresentation);
    internal static bool IsValidSymbolicNotation(string textRepresentation) => RexSymbolicNotation.IsMatch(textRepresentation);

    private static readonly Regex RexOctalDigits = RexOctalDigitsPartial();
    private static readonly Regex RexSymbolicNotation = RexSymbolicNotationPartial();

    [GeneratedRegex("^[0-7][0-7][0-7]$", RegexOptions.None)]
    private static partial Regex RexOctalDigitsPartial();
    
    [GeneratedRegex("^(a|u|ug|ugo|uo|uog|g|gu|guo|go|gou|o|ou|oug|og|ogu)[-=+](|r|rw|rwx|rx|rxw|w|wr|wrx|wx|wxr|x|xr|xrw|xw|xwr)(,(a|u|ug|ugo|uo|uog|g|gu|guo|go|gou|o|ou|oug|og|ogu)[-=+](|r|rw|rwx|rx|rxw|w|wr|wrx|wx|wxr|x|xr|xrw|xw|xwr)){0,9}$", RegexOptions.None)]
    private static partial Regex RexSymbolicNotationPartial();
}
