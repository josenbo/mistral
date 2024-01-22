using JetBrains.Annotations;

namespace vigobase;

/// <summary>
/// Signaling that the default file permissions shall be used unaltered
/// </summary>
/// <inheritdoc cref="FilePermissionType"/>
[PublicAPI]
public record FilePermissionDefault : FilePermission
{
    public override FilePermissionTypeEnum FilePermissionType => FilePermissionTypeEnum.Default;
}