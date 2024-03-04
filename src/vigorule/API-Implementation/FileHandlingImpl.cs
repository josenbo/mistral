using System.Text;
using Ardalis.GuardClauses;
using Serilog;
using vigobase;

namespace vigorule;

internal class FileHandlingImpl : IMutableFileHandling, IFinalFileHandling
{
    public bool CanDeploy { get; set; }
    public FileInfo SourceFile { get; }
    public FileInfo CheckedAndTransformedTemporaryFile => _checkedAndTransformedTemporaryFile ?? throw new VigoFatalException(AppEnv.Faults.Fatal(
        "FX574",
        $"The checked and transformed file is not (yet) available ({AppEnv.GetTopLevelRelativePath(SourceFile)})",
        string.Empty));
    public bool CheckedSuccessfully { get; private set; }
    public string RelativePathSourceFile => AppEnv.GetTopLevelRelativePath(SourceFile.FullName);
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
                        AppEnv.GetTopLevelRelativePath(SourceFile.FullName));                    
                    throw new VigoFatalException(AppEnv.Faults.Fatal("FX581","The DirectoryName of a repository file should never be null", string.Empty));
                }
        
                TargetFile = new FileInfo(Path.Combine(SourceFile.DirectoryName, _differentTargetFileName));
            }
            catch (Exception e) when (e is not VigoException)
            {
                Log.Error("Setting the new name {NewName} for the repository file {TheSourceFile} failed with an Exception",
                    _differentTargetFileName,
                    AppEnv.GetTopLevelRelativePath(SourceFile.FullName));                    
                throw new VigoFatalException(AppEnv.Faults.Fatal("FX588","Failed to set a new name for a repository file", string.Empty), e);
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

    public IEnumerable<string> DeploymentTargets => _handling.Targets;
    
    public bool HasDeploymentTarget(string target)
    {
        return _handling.Targets.Contains(target, StringComparer.InvariantCultureIgnoreCase);
    }

    public bool CanDeployForTarget(string target)
    {
        return CanDeploy && HasDeploymentTarget(target);
    }

    public void Explain(StringBuilder sb, ExplainSettings settings)
    {
        // todo: add implementation
        sb.Append(nameof(FileHandlingImpl))
            .Append('.')
            .Append(nameof(Explain))
            .AppendLine(" still needs to be implemented");
    }
    
    IFinalFileHandling IMutableFileHandling.CheckAndTransform()
    {
        var filename = SourceFile.Name;
        var filepath = AppEnv.GetTopLevelRelativePath(SourceFile.DirectoryName ?? string.Empty);
        
        if (!CanDeploy)
        {
            CheckedSuccessfully = true;
            return this;
        }

        Log.Information("The file {FileName} in {FilePath} was selected for the targets {TheTargets} by the rule {TheRule}",
            filename,
            filepath,
            _handling.Targets,
            _appliedRule.Id.RuleDescription
        );

        if (FileType == FileTypeEnum.BinaryFile)
        {
            CheckedSuccessfully = true;
            _checkedAndTransformedTemporaryFile = SourceFile;
            return this;
        }

        if (!FileType.IsDefinedAndValid() || 
            !SourceFileEncoding.IsDefinedAndValid() ||
            !TargetFileEncoding.IsDefinedAndValid() || 
            !LineEnding.IsDefinedAndValid())
        {
            Log.Fatal("Check failed for {FileName} in {FilePath} due to invalid settings {TheHandling}",
                SourceFile.Name,
                AppEnv.GetTopLevelRelativePath(SourceFile.DirectoryName ?? string.Empty),
                _handling);
            
            CheckedSuccessfully = false;
            return this;
        }

        if (!_handling.Targets.Any())
        {
            Log.Fatal("Check failed for {FileName} in {FilePath} because no targets were specified",
                SourceFile.Name,
                AppEnv.GetTopLevelRelativePath(SourceFile.DirectoryName ?? string.Empty),
                _handling);
            
            CheckedSuccessfully = false;
            return this;
        }

        try
        {
            var fileContent = File.ReadAllText(SourceFile.FullName, SourceFileEncoding.ToEncoding());
            
            if (_handling.ValidCharsRegex is not null && !_handling.ValidCharsRegex.IsMatch(fileContent))
            {
                Log.Fatal("Check failed for {FileName} in {FilePath} due to unexpected characters in the file content",
                    SourceFile.Name,
                    AppEnv.GetTopLevelRelativePath(SourceFile.DirectoryName ?? string.Empty));

                CheckedSuccessfully = false;
                return this;
            }

            var sb = new StringBuilder(fileContent);

            sb.Replace("\r\n", "\n");

            if (0 < sb.Length && _handling.FixTrailingNewline && sb[^1] != '\n')
            {
                sb.Append('\n');
            }

            if (_handling.LineEnding == LineEndingEnum.CR_LF)
                sb.Replace("\n", "\r\n");

            var tempFilePath = AppEnv.GetTemporaryFilePath();
            
            File.WriteAllText(tempFilePath, sb.ToString(), _handling.TargetFileEncoding.ToEncoding());
            
            CheckedSuccessfully = true;
            _checkedAndTransformedTemporaryFile = new FileInfo(tempFilePath);
            
            return this;
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Check failed for {FileName} in {FilePath} because an exception occured",
                SourceFile.Name,
                AppEnv.GetTopLevelRelativePath(SourceFile.DirectoryName ?? string.Empty));

            CheckedSuccessfully = false;
            return this;
        }
    }
    
    internal FileHandlingImpl(FileInfo sourceFile, FileHandlingParameters defaults, FileRule appliedRule)
    {
        _handling = defaults;
        TargetFile = SourceFile = sourceFile;
        _appliedRule = appliedRule;
    }
    
    private FileHandlingParameters _handling;
    private string? _differentTargetFileName;
    private FileInfo? _checkedAndTransformedTemporaryFile;
    private readonly FileRule _appliedRule;
}