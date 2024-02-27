using JetBrains.Annotations;

namespace vigobase;

[PublicAPI]
public static class AppEnv
{
    public static FileHandlingParameters DefaultFileHandlingParams { get; set; }
    public static StandardFileHandling DeployConfigRule { get; set; }
    public static StandardFileHandling FinalCatchAllRule { get; set; }
    public static DirectoryInfo TopLevelDirectory
    {
        get => _topLevelDirectory ?? throw new VigoFatalException($"{nameof(vigobase)}.{nameof(AppEnv)}.{nameof(TopLevelDirectory)} was not set");
        set => _topLevelDirectory = value;
    }
    public static DirectoryInfo TemporaryDirectory { get; set; }

    public static string GetTopLevelRelativePath(string path)
    {
        return Path.GetRelativePath(TopLevelDirectory.FullName, path);
    }
    
    public static string GetTopLevelRelativePath(FileSystemInfo file)
    {
        return Path.GetRelativePath(TopLevelDirectory.FullName, file.FullName);
    }

    public static string GetTemporaryFilePath()
    {
        return Path.Combine(TemporaryDirectory.FullName, $"tempfile_{_tempFileSequence++}");
    }

    static AppEnv()
    {
        var asciiGerman = ValidCharactersHelper.ParseConfiguration("AsciiGerman");
    
        DefaultFileHandlingParams = new FileHandlingParameters(
            StandardModeForFiles: (UnixFileMode)0b_110_110_100,
            StandardModeForDirectories: (UnixFileMode)0b_111_111_101,
            FileType: FileTypeEnum.BinaryFile, 
            SourceFileEncoding: FileEncodingEnum.UTF_8,
            TargetFileEncoding: FileEncodingEnum.UTF_8,
            LineEnding: LineEndingEnum.LF,
            Permissions: FilePermission.UseDefault, 
            FixTrailingNewline: true,
            ValidCharsRegex: asciiGerman,
            Targets: [ "Prod", "NonProd" ]
        );

        FinalCatchAllRule = new StandardFileHandling(
            Filenames: Array.Empty<ConfigurationFilename>(),
            Handling: DefaultFileHandlingParams with 
            {
                FileType = FileTypeEnum.BinaryFile,
                Permissions = FilePermission.UseDefault
            },
            DoCopy:false);

        DeployConfigRule = new StandardFileHandling(
            Filenames: new List<ConfigurationFilename>()
            {
                new ConfigurationFilename("deployment-rules.md", ConfigurationFileTypeEnum.MarkdownFormat),
                new ConfigurationFilename("deployment-rules.vigo", ConfigurationFileTypeEnum.NativeFormat)
            },
            Handling: DefaultFileHandlingParams with
            {
                FileType = FileTypeEnum.BinaryFile,
                Permissions = FilePermission.UseDefault
            }, 
            DoCopy: false);

        TemporaryDirectory = new DirectoryInfo(Path.GetTempPath());
    }
    
    private static DirectoryInfo? _topLevelDirectory;
    private static int _tempFileSequence = Random.Shared.Next(100000000, 999999999);
}