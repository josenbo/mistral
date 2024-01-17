using Ardalis.GuardClauses;
using vigocfg;

namespace vigolib;

internal class TargetFileForFolder : TargetFile
{
    internal string FileGroup { get; }

    internal override bool CheckBeforeCopy()
    {
        return !CopyAndTransformComplete;
    }

    internal override bool CheckAfterCopy()
    {
        return CopyAndTransformComplete;
    }
    
    public TargetFileForFolder(
        FileInfo source, 
        FileInfo target, 
        FileTypeEnum sourceFileType, 
        FileEncodingEnum sourceEncoding, 
        FilePermission targetFilePermission, 
        FileEncodingEnum targetEncoding, 
        LineEndingEnum targetLineEnding, 
        bool appendFinalNewline, 
        IEnumerable<string> tags,
        string fileGroup
    ) : base(source, target, sourceFileType, sourceEncoding, targetFilePermission, targetEncoding, targetLineEnding, appendFinalNewline, tags)
    {
        FileGroup = Guard.Against.NullOrWhiteSpace(fileGroup);
    }
    
    public void Deconstruct(out FileInfo target, out FileInfo source, out string fileGroup, out FileTypeEnum sourceFileType, out FileEncodingEnum sourceEncoding, out FilePermission targetFilePermission, out FileEncodingEnum targetEncoding, out LineEndingEnum targetLineEnding, out bool appendFinalNewline, out bool copyAndTransformComplete, out IEnumerable<string> tags)
    {
        target = Target;
        source = Source;
        fileGroup = FileGroup;
        sourceFileType = SourceFileType;
        sourceEncoding = SourceEncoding;
        targetFilePermission = TargetFilePermission;
        targetEncoding = TargetEncoding;
        targetLineEnding = TargetLineEnding;
        appendFinalNewline = AppendFinalNewline;
        copyAndTransformComplete = CopyAndTransformComplete;
        tags = Tags;
    }
}