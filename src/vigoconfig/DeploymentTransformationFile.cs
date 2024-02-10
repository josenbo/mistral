using Ardalis.GuardClauses;
using Serilog;
using vigobase;

namespace vigoconfig;

internal class DeploymentTransformationFile : IDeploymentTransformationReadWriteFile, IDeploymentTransformationReadOnlyFile
{
    public bool CanDeploy { get; set; }
    public FileInfo SourceFile { get; }
    public string RelativePathSourceFile => _handling.Settings.GetRepoRelativePath(SourceFile.FullName);
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
                        _handling.Settings.GetRepoRelativePath(SourceFile.FullName));                    
                    throw new VigoFatalException("The DirectoryName of a repository file should never be null");
                }
        
                TargetFile = new FileInfo(Path.Combine(SourceFile.DirectoryName, _differentTargetFileName));
            }
            catch (Exception e) when (e is not VigoException)
            {
                Log.Error(e,"Setting the new name {NewName} for the repository file {TheSourceFile} failed with an Exception",
                    _differentTargetFileName,
                    _handling.Settings.GetRepoRelativePath(SourceFile.FullName));                    
                throw new VigoFatalException("Failed to set a new name for a repository file", e);
            }
        }
    }
    public FileInfo TargetFile { get; private set; }

    public FileTypeEnum FileType
    {
        get => _handling.FileType;
        set => _handling = _handling with { FileType = Guard.Against.InvalidInput(value, nameof(value), FileTypeEnumHelper.IsDefinedAndValid) };
    }

    public FileEncodingEnum SourceFileEncoding
    {
        get => _handling.SourceFileEncoding;
        set => _handling = _handling with { SourceFileEncoding = Guard.Against.InvalidInput(value, nameof(value), FileEncodingEnumHelper.IsDefinedAndValid) };
    }

    public FileEncodingEnum TargetFileEncoding
    {
        get => _handling.TargetFileEncoding;
        set => _handling = _handling with { TargetFileEncoding = Guard.Against.InvalidInput(value, nameof(value), FileEncodingEnumHelper.IsDefinedAndValid) };
    }

    public FilePermission FilePermission
    {
        get => _handling.Permissions;
        set => _handling = _handling with { Permissions = value };
    }

    public LineEndingEnum LineEnding
    {
        get => _handling.LineEnding;
        set => _handling = _handling with { LineEnding = Guard.Against.InvalidInput(value, nameof(value), LineEndingEnumHelper.IsDefinedAndValid) };
    }

    public bool FixTrailingNewline
    {
        get => _handling.FixTrailingNewline; 
        set => _handling = _handling with { FixTrailingNewline = value };
    }

    IDeploymentTransformationReadOnlyFile IDeploymentTransformationReadWriteFile.GetReadOnlyInterface()
    {
        return this;
    }

    internal DeploymentTransformationFile(FileInfo sourceFile, FileHandlingParameters defaults)
    {
        _handling = defaults;
        TargetFile = SourceFile = sourceFile;
    }

    private FileHandlingParameters _handling;
    private string? _differentTargetFileName;
}