using Ardalis.GuardClauses;
using vigocfg;

namespace vigolib;

internal abstract class TargetFile
{
    internal string Key => Target.FullName;
    internal FileInfo Target { get; }
    internal FileInfo Source { get; }
    internal IEnumerable<string> Tags => _tags;
    internal FileTypeEnum SourceFileType { get; }
    internal FileEncodingEnum SourceEncoding { get; }
    internal FilePermission TargetFilePermission { get; }
    internal FileEncodingEnum TargetEncoding { get; }
    internal LineEndingEnum TargetLineEnding { get; }
    internal bool AppendFinalNewline { get; }
    internal bool CopyAndTransformComplete { get; set; } = false;

    internal abstract bool CheckBeforeCopy();
    internal abstract bool CheckAfterCopy();
    internal TargetFile(
        FileInfo source, 
        FileInfo target, 
        FileTypeEnum sourceFileType, 
        FileEncodingEnum sourceEncoding, 
        FilePermission targetFilePermission, 
        FileEncodingEnum targetEncoding, 
        LineEndingEnum targetLineEnding, 
        bool appendFinalNewline, 
        IEnumerable<string> tags)
    {
        source.Refresh();
        Source = Guard.Against.InvalidInput(source, nameof(source), f => f.Exists);
        Target = target;
        SourceFileType = Guard.Against.InvalidInput(sourceFileType, nameof(sourceFileType), t => t.IsDefinedAndValid());
        if (SourceFileType == FileTypeEnum.BinaryFile)
        {
            SourceEncoding = sourceEncoding;
            TargetEncoding = targetEncoding;
            TargetLineEnding = targetLineEnding;
        }
        else
        {
            SourceEncoding = Guard.Against.InvalidInput(sourceEncoding, nameof(sourceEncoding), e => e.IsDefinedAndValid());
            TargetEncoding = Guard.Against.InvalidInput(targetEncoding, nameof(targetEncoding), e => e.IsDefinedAndValid());
            TargetLineEnding = Guard.Against.InvalidInput(targetLineEnding, nameof(targetLineEnding), l => l.IsDefinedAndValid());
        }
        TargetFilePermission =  targetFilePermission;
        AppendFinalNewline = appendFinalNewline;
        _tags.AddRange(tags);
    }

    private readonly List<string> _tags = new();
}