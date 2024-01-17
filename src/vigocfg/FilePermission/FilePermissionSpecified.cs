using JetBrains.Annotations;

namespace vigocfg;

/// <summary>
/// Signaling that the default file permissions shall be changed
/// </summary>
[PublicAPI]
public abstract record FilePermissionSpecified() : FilePermission
{
    /// <summary>
    /// The desired file permissions
    /// </summary>
    public abstract string TextRepresentation { get; protected set; }
}