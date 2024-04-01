using JetBrains.Annotations;
using vigobase;

namespace vigorule;

[PublicAPI]
public interface IMutableFileHandling : IMutableHandling
{
    FileInfo SourceFile { get; }
    string RelativePathSourceFile { get; }
    string? DifferentTargetFileName { get; set; }
    FileInfo TargetFile { get; }
    FileTypeEnum FileType { get; set; }
    FileEncodingEnum SourceFileEncoding { get; set; }
    FileEncodingEnum TargetFileEncoding { get; set; }
    FilePermission FilePermission { get; set; }
    LineEndingEnum LineEnding { get; set; }
    bool FixTrailingNewline { get; set; }
    IFinalFileHandling CheckAndTransform();
}