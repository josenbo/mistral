using JetBrains.Annotations;

namespace vigobase;

/// <summary>
/// Signaling that the default file permissions shall be used unaltered
/// </summary>
/// <inheritdoc cref="FilePermissionType"/>
[PublicAPI]
public record FilePermissionDefault : FilePermission
{
    /// <inheritdoc cref="FilePermission"/>
    public override FilePermissionTypeEnum FilePermissionType => FilePermissionTypeEnum.Default;

    /// <summary>
    /// This implementation exists for the sake of handling
    /// all permission types in a similar fashion. Since this
    /// permission type commands to apply the default, it will
    /// return the value in the defaultUnixFileMode parameter.  
    /// </summary>
    /// <inheritdoc cref="FilePermission"/>
    public override UnixFileMode ComputeUnixFileMode(UnixFileMode defaultUnixFileMode)
    {
        return defaultUnixFileMode;
    }
}