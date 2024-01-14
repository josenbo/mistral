using System.Text.RegularExpressions;
using Ardalis.GuardClauses;

namespace vigocfg;

public abstract partial record FilePermissionValue
{
    public static FilePermissionValueUndefined Undefined => new FilePermissionValueUndefined();

    public static FilePermissionValueDefined FromOctalText(string octalDigits)
    {
        Guard.Against.InvalidInput(octalDigits, nameof(octalDigits), s => RexOctalDigits.IsMatch(s));

        return new FilePermissionValueDefined(IsSymbolicNotation: false, TextRepresentation: octalDigits);
    }

    public static FilePermissionValueDefined FromSymbolicNotation(string symbolicNotation)
    {
        Guard.Against.InvalidInput(symbolicNotation, nameof(symbolicNotation), s => RexSymbolicNotation.IsMatch(s));

        return new FilePermissionValueDefined(IsSymbolicNotation: true, TextRepresentation: symbolicNotation);
    }

    private static readonly Regex RexOctalDigits = RexOctalDigitsPartial();
    private static readonly Regex RexSymbolicNotation = RexSymbolicNotationPartial();

    [GeneratedRegex("^[0-7][0-7][0-7]$", RegexOptions.None)]
    private static partial Regex RexOctalDigitsPartial();
    
    [GeneratedRegex("^[augo]{1,3}[-=+][rwx]{1,3}(,[augo]{1,3}[-=+][rwx]{1,3}){0,9}$", RegexOptions.None)]
    private static partial Regex RexSymbolicNotationPartial();
}

public record FilePermissionValueUndefined : FilePermissionValue;

// ReSharper disable NotAccessedPositionalProperty.Global
public record FilePermissionValueDefined(bool IsSymbolicNotation, string TextRepresentation) : FilePermissionValue;
// ReSharper restore NotAccessedPositionalProperty.Global
