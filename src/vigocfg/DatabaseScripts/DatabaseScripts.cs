using Ardalis.GuardClauses;

namespace vigocfg;

internal class DatabaseScripts : IDatabaseScripts
{
    public DirectoryInfo SourceFolder { get; }
    public DirectoryInfo? TargetFolder { get; }
    public bool CleanTargetFolder { get; }
    public FileTypeEnum SourceFileType { get; }
    public FileEncodingEnum SourceEncoding { get; }
    public FilePermission TargetFilePermission { get; }
    public FileEncodingEnum TargetEncoding { get; }
    public LineEndingEnum TargetLineEnding { get; }
    public bool TargetAppendFinalNewline { get; }

    internal DatabaseScripts(DirectoryInfo source, DirectoryInfo? target, bool cleanTarget, SourceFileProperties sourceFileProperties, TargetFileProperties targetFileProperties)
    {
        Guard.Against.InvalidInput(sourceFileProperties, nameof(sourceFileProperties), p => p.FileType.IsDefinedAndValid() && p.FileEncoding.IsDefinedAndValid());
        Guard.Against.InvalidInput(targetFileProperties, nameof(targetFileProperties), t => t.FileEncoding.IsDefinedAndValid() && t.LineEnding.IsDefinedAndValid());
        
        SourceFolder = Guard.Against.InvalidInput(source, nameof(source), s => s.Exists);
        TargetFolder = target is null
            ? null
            : Guard.Against.InvalidInput(target, nameof(target), t => t.Exists);
        CleanTargetFolder = cleanTarget;
        SourceFileType = sourceFileProperties.FileType;
        SourceEncoding = sourceFileProperties.FileEncoding;
        TargetFilePermission = targetFileProperties.FilePermission;
        TargetEncoding = targetFileProperties.FileEncoding;
        TargetLineEnding = targetFileProperties.LineEnding;
        TargetAppendFinalNewline = targetFileProperties.AppendFinalNewline;
    }
}