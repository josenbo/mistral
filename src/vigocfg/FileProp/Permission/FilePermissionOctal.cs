using Ardalis.GuardClauses;

namespace vigocfg;

/// <summary>
/// Signaling that the default file permissions shall be changed as specified using octal notation
/// </summary>
/// <inheritdoc cref="FilePermissionType"/>
/// <inheritdoc cref="TextRepresentation"/>
public record FilePermissionOctal(string TextRepresentation) : FilePermissionSpecified
{
    public override string TextRepresentation { get; protected set; } =
        Guard.Against.InvalidInput(TextRepresentation, nameof(TextRepresentation), IsValidOctalNotation);
    public override FilePermissionTypeEnum FilePermissionType => FilePermissionTypeEnum.Octal;
}