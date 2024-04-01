using System.Globalization;
using Ardalis.GuardClauses;
using JetBrains.Annotations;

namespace vigobase;

/// <summary>
/// Signaling that the default file permissions shall be changed as specified using octal notation
/// </summary>
/// <inheritdoc cref="FilePermissionType"/>
[PublicAPI]
public record FilePermissionOctal(string TextRepresentation) : FilePermissionSpecified
{
    /// <summary>
    /// A string with three octal [0-7] digits representing the unix file mode flags
    /// for user, group and others (see the chmod man pages on a unix box).
    /// This will be turned into an integral number type on the fly as needed.
    /// </summary>
    public override string TextRepresentation { get; protected set; } =
        Guard.Against.InvalidInput(TextRepresentation, nameof(TextRepresentation), IsValidOctalNotation);

    /// <summary>
    /// Read-only conversion turning the textual representation
    /// into the corresponding file mode flags. This relies on
    /// the fact that the unix file mode is an integer with
    /// bit flags for read, write and execute in three groups
    /// for user, group and others where each group occupies
    /// three bits. See the chmod man pages on a unix box for
    /// further information.
    /// </summary>
    public UnixFileMode NumericRepresentation => (UnixFileMode) (
        CharUnicodeInfo.GetDecimalDigitValue(TextRepresentation[0]) << 6 |
        CharUnicodeInfo.GetDecimalDigitValue(TextRepresentation[1]) << 3 |
        CharUnicodeInfo.GetDecimalDigitValue(TextRepresentation[2])
    );
    /// <inheritdoc cref="FilePermissionType"/>
    public override FilePermissionTypeEnum FilePermissionType => FilePermissionTypeEnum.Octal;

    /// <summary>
    /// This implementation exists for the sake of handling
    /// all permission types alike. Since this permission type
    /// defines the effective file mode, the default is ignored
    /// and the effective file mode is returned.  
    /// </summary>
    /// <inheritdoc cref="FilePermission"/>
    public override UnixFileMode ComputeUnixFileMode(UnixFileMode defaultUnixFileMode)
    {
        return NumericRepresentation;
    }

}