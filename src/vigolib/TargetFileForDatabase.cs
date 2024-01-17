using Ardalis.GuardClauses;
using vigocfg;

namespace vigolib;

internal class TargetFileForDatabase : TargetFile
{
    internal string SchemaName { get; }
    internal DatabaseObjectTypeEnum DatabaseObjectType { get; }
    
    internal override bool CheckBeforeCopy()
    {
        return !CopyAndTransformComplete;
    }

    internal override bool CheckAfterCopy()
    {
        return CopyAndTransformComplete;
    }
    
    public TargetFileForDatabase(
        FileInfo source, 
        FileInfo target, 
        string schemaName,
        DatabaseObjectTypeEnum databaseObjectType,
        FileTypeEnum sourceFileType, 
        FileEncodingEnum sourceEncoding, 
        FilePermission targetFilePermission, 
        FileEncodingEnum targetEncoding, 
        LineEndingEnum targetLineEnding, 
        bool appendFinalNewline, 
        IEnumerable<string> tags
    ) : base(source, target, sourceFileType, sourceEncoding, targetFilePermission, targetEncoding, targetLineEnding, appendFinalNewline, tags)
    {
        SchemaName = Guard.Against.NullOrWhiteSpace(schemaName);
        DatabaseObjectType = Guard.Against.InvalidInput(databaseObjectType, nameof(databaseObjectType), t => t.IsDefinedAndValid());
    }
    
    public void Deconstruct(out FileInfo target, out FileInfo source, out string schemaName, out DatabaseObjectTypeEnum databaseObjectType, out FileTypeEnum sourceFileType, out FileEncodingEnum sourceEncoding, out FilePermission targetFilePermission, out FileEncodingEnum targetEncoding, out LineEndingEnum targetLineEnding, out bool appendFinalNewline, out bool copyAndTransformComplete, out IEnumerable<string> tags)
    {
        target = Target;
        source = Source;
        schemaName = SchemaName;
        databaseObjectType = DatabaseObjectType;
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