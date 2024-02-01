using Ardalis.GuardClauses;
using Serilog;
using vigobase;

namespace vigoconfig;

internal class DeploymentTransformationFile : IDeploymentTransformationReadWriteFile, IDeploymentTransformationReadOnlyFile
{
    public bool CanDeploy { get; set; }
    public FileInfo SourceFile { get; }
    public string RelativePathSourceFile => _defaults.GetRepositoryRelativePath(SourceFile.FullName);
    public string? DifferentTargetFileName
    {
        get => _differentTargetFileName;
        set
        {
            if (_differentTargetFileName == value) return;

            try
            {
                _differentTargetFileName = string.IsNullOrEmpty(value) ? null : value;

                if (_differentTargetFileName == null || _differentTargetFileName == SourceFile.Name)
                {
                    TargetFile = SourceFile;
                    return;
                }

                if (SourceFile.DirectoryName is null)
                {
                    Log.Error("Setting the new name {NewName} for the repository file {TheSourceFile} failed, because the repository file has no parent directory",
                        _differentTargetFileName,
                        _defaults.GetRepositoryRelativePath(SourceFile.FullName));                    
                    throw new VigoFatalException("The DirectoryName of a repository file should never be null");
                }
        
                TargetFile = new FileInfo(Path.Combine(SourceFile.DirectoryName, _differentTargetFileName));
            }
            catch (Exception e) when (e is not VigoException)
            {
                Log.Error(e,"Setting the new name {NewName} for the repository file {TheSourceFile} failed with an Exception",
                    _differentTargetFileName,
                    _defaults.GetRepositoryRelativePath(SourceFile.FullName));                    
                throw new VigoFatalException("Failed to set a new name for a repository file", e);
            }
        }
    }
    public FileInfo TargetFile { get; private set; }

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

    IDeploymentTransformationReadOnlyFile IDeploymentTransformationReadWriteFile.GetReadOnlyInterface()
    {
        return this;
    }

    internal DeploymentTransformationFile(FileInfo sourceFile, DeploymentDefaults defaults)
    {
        _defaults = defaults;
        TargetFile = SourceFile = sourceFile;
        FileType = defaults.FileTypeDefault;
        SourceFileEncoding = defaults.SourceFileEncodingDefault;
        TargetFileEncoding = defaults.TargetFileEncodingDefault;
        FilePermission = FilePermission.UseDefault;
        LineEnding = defaults.LineEndingDefault;
        FixTrailingNewline = defaults.TrailingNewlineDefault;
    }

    private string? _differentTargetFileName;
    private FileTypeEnum _fileType;
    private FileEncodingEnum _sourceFileEncoding;
    private FileEncodingEnum _targetFileEncoding;
    private LineEndingEnum _lineEnding;
    private readonly DeploymentDefaults _defaults;
}