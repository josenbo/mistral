using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace vigobase;

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
    /// Apply the specified Permissions on the default
    /// unix file mode and return the calculated value  
    /// </summary>
    /// <param name="defaultUnixFileMode">
    /// A default for the file mode. It is the callers
    /// responsibility to provide different defaults
    /// for file and directory permissions. 
    /// </param>
    /// <returns>The combination of the given default and the specified permission</returns>
    /// <example>
    /// Define a default 644 file mode and a symbolic
    /// permission to give the user execute rights.
    /// Computing the unix file mode from these values
    /// will add UnixFileMode.UserExecute and result to 744.  
    /// <code>
    /// var defaultFileMode = UnixFileMode.UserRead |
    ///                       UnixFileMode.UserWrite |
    ///                       UnixFileMode.GroupRead |
    ///                       UnixFileMode.OtherRead;
    /// var symbolicPerm = new FilePermissionSymbolic("u+x");
    /// var combinedFileMode = symbolicPerm.ComputeUnixFileMode(defaultFileMode);
    /// </code>
    /// </example>
    public abstract UnixFileMode ComputeUnixFileMode(UnixFileMode defaultUnixFileMode); 
    
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

    /// <summary>
    /// Check if the given text is a valid octal or symbolic notation
    /// and, if that is the case, return a permission object of that type.
    /// If the text is empty, return the default permission. 
    /// </summary>
    /// <param name="text">The text to parse for a valid notation</param>
    /// <param name="result">Null, if the notation was not recognized or a permission object with the appropriate subtype</param>
    /// <returns>True, if the notation was recognized</returns>
    public static bool TryParse(string? text, [NotNullWhen(true)] out FilePermission? result)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            result = UseDefault;
            return true;
        }

        if (IsValidOctalNotation(text))
        {
            result = new FilePermissionOctal(text);
            return true;
        }
        
        if (IsValidSymbolicNotation(text))
        {
            result = new FilePermissionSymbolic(text);
            return true;
        }

        result = null;
        return false;
    }
    
    internal static bool IsValidOctalNotation(string textRepresentation) => RexOctalDigits.IsMatch(textRepresentation);
    internal static bool IsValidSymbolicNotation(string textRepresentation)
    {
        if (string.IsNullOrWhiteSpace(textRepresentation) || 40 < textRepresentation.Length)
            return false;

        var numberOfExpressions = 0;
        // split the group of symbolic expressions on the comma delimiter
        // and handle each expression separately 
        foreach (var expr in textRepresentation.Split(',', StringSplitOptions.None))
        {
            // accept a maximum of 10 expressions
            if (10 < ++numberOfExpressions) return false;
            
            if (!RexSymbolicNotation.IsMatch(expr)) return false;
        }    
        
        return (0 < numberOfExpressions);
    }

    private static readonly Regex RexOctalDigits = RexOctalDigitsPartial();
    private static readonly Regex RexSymbolicNotation = RexSymbolicNotationPartial();

    [GeneratedRegex("^[0-7][0-7][0-7]$", RegexOptions.None)]
    private static partial Regex RexOctalDigitsPartial();
    
    [GeneratedRegex("^(((|a|u|ug|ugo|uo|uog|g|gu|guo|go|gou|o|ou|oug|og|ogu)=(|r|rw|rwx|rx|rxw|w|wr|wrx|wx|wxr|x|xr|xrw|xw|xwr))|((|a|u|ug|ugo|uo|uog|g|gu|guo|go|gou|o|ou|oug|og|ogu)[-+](r|rw|rwx|rx|rxw|w|wr|wrx|wx|wxr|x|xr|xrw|xw|xwr)))$$", RegexOptions.None)]
    private static partial Regex RexSymbolicNotationPartial();
}
