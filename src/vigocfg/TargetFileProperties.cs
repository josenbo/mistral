namespace vigocfg;

internal record TargetFileProperties(
    FileEncodingEnum FileEncoding,
    LineEndingEnum LineEnding,
    bool AppendFinalNewline,
    FilePermissionValue FilePermission
);