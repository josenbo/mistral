namespace vigobase;

public interface IDeploymentTransformationReadOnlyFile : IDeploymentTransformationReadOnly
{
    FileInfo SourceFile { get; }
    FileInfo CheckedAndTransformedTemporaryFile { get; }
    string RelativePathSourceFile { get; }
    string? DifferentTargetFileName { get; }
    FileInfo TargetFile { get; }
    FileTypeEnum FileType { get; }
    FileEncodingEnum SourceFileEncoding { get; }
    FileEncodingEnum TargetFileEncoding { get; }
    FilePermission FilePermission { get; }
    LineEndingEnum LineEnding { get; }
    IEnumerable<string> DeploymentTargets { get; }
    bool CanDeploy { get; }
    bool FixTrailingNewline { get; }
}
