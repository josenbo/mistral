using Ardalis.GuardClauses;
using Serilog;

namespace vigobase;

public record FileTransformDescription(
    FileTypeEnum FileType,
    FileInfo SourceFile,
    FileEncodingEnum SourceFileEncoding,
    FileInfo TargetFile,
    FileEncodingEnum TargetFileEncoding,
    FilePermission FilePermission,
    LineEndingEnum LineEnding,
    bool FixTrailingNewline
);

public class DeploymentTransformation
{
    public FileInfo SourceFile { get; }
    public string? DifferentTargetFileName
    {
        get => _differentTargetFileName;
        set
        {
            if (_differentTargetFileName == value) return;

            try
            {
                _differentTargetFileName = string.IsNullOrEmpty(value) ? null : value;

                if (_differentTargetFileName == null || _differentTargetFileName != SourceFile.Name)
                {
                    _targetFile = SourceFile;
                    return;
                }

                if (SourceFile.DirectoryName is null)
                {
                    Log.Error("Setting the new name {NewName} for the repository file {TheSourceFile} failed, because the repository file has no parent directory",
                        _differentTargetFileName,
                        SourceFile);                    
                    throw new VigoFatalException("The DirectoryName of a repository file should never be null");
                }
        
                _targetFile = new FileInfo(Path.Combine(SourceFile.DirectoryName, _differentTargetFileName));
            }
            catch (Exception e) when (e is not VigoException)
            {
                Log.Error(e,"Setting the new name {NewName} for the repository file {TheSourceFile} failed with an Exception",
                    _differentTargetFileName,
                    SourceFile);                    
                throw new VigoFatalException("Failed to set a new name for a repository file", e);
            }
        }
    }
    public FileInfo TargetFile => _targetFile;

    public FileTypeEnum FileType
    {
        get => _fileType;
        set => _fileType = Guard.Against.InvalidInput(value, nameof(value), FileTypeEnumHelper.IsDefinedAndValid);
    }

    public FileEncodingEnum SourceFileEncoding
    {
        get => _sourceFileEncoding;
        set => _sourceFileEncoding = Guard.Against.InvalidInput(value, nameof(value), FileEncodingEnumHelper.IsDefinedAndValid);
    }

    public FileEncodingEnum TargetFileEncoding
    {
        get => _targetFileEncoding;
        set => _targetFileEncoding = Guard.Against.InvalidInput(value, nameof(value), FileEncodingEnumHelper.IsDefinedAndValid);
    }

    public FilePermission FilePermission { get; set; }

    public LineEndingEnum LineEnding
    {
        get => _lineEnding;
        set => _lineEnding = Guard.Against.InvalidInput(value, nameof(value), LineEndingEnumHelper.IsDefinedAndValid);
    }

    public bool FixTrailingNewline { get; set; }
    
    public DeploymentTransformation(FileInfo sourceFile, DeploymentDefaults defaults)
    {
        _targetFile = SourceFile = sourceFile;
        FileType = defaults.FileTypeDefault;
        SourceFileEncoding = defaults.SourceFileEncodingDefault;
        TargetFileEncoding = defaults.TargetFileEncodingDefault;
        FilePermission = FilePermission.UseDefault;
        LineEnding = defaults.LineEndingDefault;
        FixTrailingNewline = defaults.TrailingNewlineDefault;
    }

    private FileInfo _targetFile;
    private string? _differentTargetFileName;
    private FileTypeEnum _fileType;
    private FileEncodingEnum _sourceFileEncoding;
    private FileEncodingEnum _targetFileEncoding;
    private LineEndingEnum _lineEnding;
}