using Ardalis.GuardClauses;

namespace vigobase;

/// <summary>
/// Signaling that the default file permissions shall be changed as specified using symbolic notation
/// </summary>
/// <inheritdoc cref="FilePermissionType"/>
/// <inheritdoc cref="TextRepresentation"/>
public record FilePermissionSymbolic(string TextRepresentation) : FilePermissionSpecified
{
    public override string TextRepresentation { get; protected set; } =
        Guard.Against.InvalidInput(TextRepresentation, nameof(TextRepresentation), IsValidSymbolicNotation);
    public override FilePermissionTypeEnum FilePermissionType => FilePermissionTypeEnum.Symbolic;
}