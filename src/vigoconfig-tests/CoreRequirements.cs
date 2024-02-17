namespace vigoconfig_tests;

public class CoreRequirements
{
    [Fact]
    public void ReadMinimalMarkdownFormat()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """

                               #  [vîgô] Some title

                               markdown hash + key phrase = markdown format
                               Must be on the first non-empty line to be 
                               recognized
                               
                               """;
        
        var folderConfig = FolderConfigReader.Parse(content, _defaultHandling, _deployConfigRule, _finalCatchAllRule);
        
        Assert.True(folderConfig is not null);
    }

    [Fact]
    public void ReadMinimalNativeFormat()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                               
                                //  vîgô some other text
                                
                               native comment token + key phrase = native format
                               Need not be on the first line but at least once
                               in the content

                               """;
        
        var folderConfig = FolderConfigReader.Parse(content, _defaultHandling, _deployConfigRule, _finalCatchAllRule);
        
        Assert.True(folderConfig is not null);
    }
    
    [Fact]
    public void RejectUnrecognizableFormat()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               //
                               vîgô
                               # vîgô
                               
                               Has the key phrase but not the native comment 
                               on the same line. The markdown is not recognized
                               because hash and key phrase are not on the first
                               non-empty line
                               
                               """;
        
        Assert.Throws<VigoParseFolderConfigException>(() => FolderConfigReader.Parse(content, _defaultHandling, _deployConfigRule, _finalCatchAllRule));
    }
    
    #region Helpers

    public CoreRequirements(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        _logLevelSwitch = new LoggingLevelSwitch
        {
            MinimumLevel = LogEventLevel.Information
        };

        Log.Logger = new LoggerConfiguration()
            // add the xunit test output sink to the serilog logger
            // https://github.com/trbenning/serilog-sinks-xunit#serilog-sinks-xunit
            .WriteTo.TestOutput(testOutputHelper)
            .MinimumLevel.ControlledBy(_logLevelSwitch)
            .CreateLogger();
        
        _defaultHandling = new FileHandlingParameters(
            Settings: new TestAppSettings(),
            StandardModeForFiles: (UnixFileMode)0b_110_110_100,
            StandardModeForDirectories: (UnixFileMode)0b_111_111_101,
            FileType: FileTypeEnum.BinaryFile, 
            SourceFileEncoding: FileEncodingEnum.UTF_8,
            TargetFileEncoding: FileEncodingEnum.UTF_8,
            LineEnding: LineEndingEnum.LF,
            Permissions: FilePermission.UseDefault, 
            FixTrailingNewline: true,
            ValidCharsRegex: ValidCharactersHelper.ParseConfiguration("AsciiGerman"),
            Targets: [ "Prod", "NonProd" ]
        );
        
        _deployConfigRule = new StandardFileHandling(
            Filenames: new string[]{ "deploy.one", "deploy2", "deploy-three" },
            DoCopy: false,
            Handling: _defaultHandling
        );

        _finalCatchAllRule = new StandardFileHandling(
            Filenames: Array.Empty<string>(),
            DoCopy: false,
            Handling: _defaultHandling
        );
    }
    
    // ReSharper disable once NotAccessedField.Local
    private readonly ITestOutputHelper _testOutputHelper;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly LoggingLevelSwitch _logLevelSwitch;
    private readonly FileHandlingParameters _defaultHandling;
    private readonly StandardFileHandling _deployConfigRule;
    private readonly StandardFileHandling _finalCatchAllRule;

    #endregion
}