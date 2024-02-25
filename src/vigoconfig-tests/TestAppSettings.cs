namespace vigoconfig_tests;

internal class TestAppSettings : IAppSettings
{
    public CommandEnum Command => throw new VigoFatalException("Unit tests must not use this property");

    public DirectoryInfo RepositoryRoot => throw new VigoFatalException("Unit tests must not use this property");

    public FileHandlingParameters DefaultFileHandlingParams { get; set; }
    public StandardFileHandling DeployConfigRule { get; set; }
    public StandardFileHandling FinalCatchAllRule { get; set; }
    public string GetRepoRelativePath(string path) => throw new VigoFatalException("Unit tests must not use this method");
    public string GetRepoRelativePath(FileSystemInfo file) => throw new VigoFatalException("Unit tests must not use this method");
    public string GetTemporaryFilePath() => throw new VigoFatalException("Unit tests must not use this method");

    public TestAppSettings()
    {
        var asciiGerman = ValidCharactersHelper.ParseConfiguration("AsciiGerman");
        
        DefaultFileHandlingParams = new FileHandlingParameters(
            Settings: this,
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
            DoCopy: false);

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
    }        
}