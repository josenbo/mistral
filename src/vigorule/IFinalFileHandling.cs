using JetBrains.Annotations;
using vigobase;

namespace vigorule;

[PublicAPI]
public interface IFinalFileHandling : IFinalHandling
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
    bool FixTrailingNewline { get; }
}
