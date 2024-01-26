namespace vigobase;

public interface IDeploymentTransformationReadWrite 
{
    FileInfo SourceFile { get; }
    string? DifferentTargetFileName { get; set; }
    FileInfo TargetFile { get; }
    FileTypeEnum FileType { get; set; }
    FileEncodingEnum SourceFileEncoding { get; set; }
    FileEncodingEnum TargetFileEncoding { get; set; }
    FilePermission FilePermission { get; set; }
    LineEndingEnum LineEnding { get; set; }
    bool FixTrailingNewline { get; set; }
    IDeploymentTransformationReadOnly GetReadOnlyInterface();
}