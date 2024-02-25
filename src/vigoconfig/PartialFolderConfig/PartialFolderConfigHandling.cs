using System.Text.RegularExpressions;
using vigobase;

namespace vigoconfig;

internal class PartialFolderConfigHandling
{
    public UnixFileMode? StandardModeForFiles { get; set; }
    public UnixFileMode? StandardModeForDirectories { get; set; } 
    public FileTypeEnum? FileType { get; set; } 
    public FileEncodingEnum? SourceFileEncoding { get; set; }
    public FileEncodingEnum? TargetFileEncoding { get; set; }
    public LineEndingEnum? LineEnding { get; set; }
    public FilePermission? Permissions { get; set; }
    public bool? FixTrailingNewline { get; set; }
    public bool IsDefinedValidCharsRegex { get; set; }
    public Regex? ValidCharsRegex { get; set; } 
    public IReadOnlyList<string>? Targets { get; set; }

    public PartialFolderConfigHandling()
    {
        
    }

    public PartialFolderConfigHandling(PartialFolderConfigHandling defaultParameters)
    {
        StandardModeForFiles = StandardModeForFiles ?? defaultParameters.StandardModeForFiles;
        StandardModeForDirectories = StandardModeForDirectories ?? defaultParameters.StandardModeForDirectories;
        FileType = FileType ?? defaultParameters.FileType;
        SourceFileEncoding = SourceFileEncoding ?? defaultParameters.SourceFileEncoding;
        TargetFileEncoding = TargetFileEncoding ?? defaultParameters.TargetFileEncoding;
        LineEnding = LineEnding ?? defaultParameters.LineEnding;
        Permissions = Permissions ?? defaultParameters.Permissions;
        FixTrailingNewline = FixTrailingNewline ?? defaultParameters.FixTrailingNewline;
        IsDefinedValidCharsRegex = IsDefinedValidCharsRegex || defaultParameters.IsDefinedValidCharsRegex;
        ValidCharsRegex = IsDefinedValidCharsRegex ? ValidCharsRegex : defaultParameters.ValidCharsRegex;
        Targets = Targets ?? defaultParameters.Targets;
    }
    public FileHandlingParameters Apply(FileHandlingParameters defaultParameters)
    {
        return defaultParameters with
        {
            StandardModeForFiles = StandardModeForFiles ?? defaultParameters.StandardModeForFiles,
            StandardModeForDirectories = StandardModeForDirectories ?? defaultParameters.StandardModeForDirectories,
            FileType = FileType ?? defaultParameters.FileType,
            SourceFileEncoding = SourceFileEncoding ?? defaultParameters.SourceFileEncoding,
            TargetFileEncoding = TargetFileEncoding ?? defaultParameters.TargetFileEncoding,
            LineEnding = LineEnding ?? defaultParameters.LineEnding,
            Permissions = Permissions ?? defaultParameters.Permissions,
            FixTrailingNewline = FixTrailingNewline ?? defaultParameters.FixTrailingNewline,
            ValidCharsRegex = IsDefinedValidCharsRegex ? ValidCharsRegex : defaultParameters.ValidCharsRegex,
            Targets = Targets ?? defaultParameters.Targets
        };
    }

    public void Deconstruct(out UnixFileMode? standardModeForFiles, out UnixFileMode? standardModeForDirectories, out FileTypeEnum? fileType, out FileEncodingEnum? sourceFileEncoding, out FileEncodingEnum? targetFileEncoding, out LineEndingEnum? lineEnding, out FilePermission? permissions, out bool? fixTrailingNewline, out bool isDefinedValidCharsRegex, out Regex? validCharsRegex, out IReadOnlyList<string>? targets)
    {
        standardModeForFiles = StandardModeForFiles;
        standardModeForDirectories = StandardModeForDirectories;
        fileType = FileType;
        sourceFileEncoding = SourceFileEncoding;
        targetFileEncoding = TargetFileEncoding;
        lineEnding = LineEnding;
        permissions = Permissions;
        fixTrailingNewline = FixTrailingNewline;
        isDefinedValidCharsRegex = IsDefinedValidCharsRegex;
        validCharsRegex = ValidCharsRegex;
        targets = Targets;
    }
}