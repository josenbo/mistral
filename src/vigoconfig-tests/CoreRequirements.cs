namespace vigoconfig_tests;

public class CoreRequirements
{
    [Fact]
    public void ReadMinimalMarkdownFormat()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """

                               #  [vîgô] Some title

                               hash followed by key phrase on the first 
                               non-empty line = markdown format
                               
                               """;
        
        var folderConfig = FolderConfigReader.Parse(content);
        
        Assert.True(folderConfig is not null);
    }

    [Fact]
    public void ReadMinimalNativeFormat()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                               
                                #  vîgô some other text
                                
                               # shebang on the first line plus hash followed 
                               # by key phrase anywhere at least once = native format

                               """;
        
        var folderConfig = FolderConfigReader.Parse(content);
        
        Assert.True(folderConfig is not null);
    }
    
    [Fact]
    public void RejectUnrecognizableFormat()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               
                               other content
                               # vîgô
                               
                               Has hash followed by key phrase alright, but it
                               is neither on the first non-empty line, nor is 
                               the shebang present
                               
                               """;
        
        Assert.Throws<VigoParseFolderConfigException>(() => FolderConfigReader.Parse(content));
    }

    [Fact]
    public void ReadMarkdownWithCommentsOnly()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #vîgô

                               this is no vigo block and will be ignored
                               
                               ```
                               this will be ignored
                               ```
                               
                               this is a bash block which will be ignored
                               
                               ```
                               bash is ignored
                               ```
                               
                               ```   vigo   some other text here
                               
                               #comments and empty lines will be ignored
                               
                               # end
                               ```
                               
                               end of document
                               """;
        
        var folderConfig = FolderConfigReader.Parse(content);
        
        Assert.Null(folderConfig.KeepEmptyFolder);
        Assert.Null(folderConfig.LocalDefaults);
        Assert.Empty(folderConfig.PartialRules);
    }

    [Fact]
    public void ReadNativeWithCommentsOnly()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                               #vîgô

                               DO IGNORE TEXT FILE
                                   SomeValue 
                               DONE
                               
                               CONFIGURE FOLDER
                                   doing folder 
                                   configuration
                               DONE
                               
                               do deploy text file 
                                   if name is hubert
                                   do something with hubert
                               done
                               
                               """;
        
        var folderConfig = FolderConfigReader.Parse(content);
        
        
        Assert.True(folderConfig is not null);
    }

    [Fact]
    public void AtMostOneSingleFolderBlock()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                                                   # vîgô.
                                
                               configure folder
                               done
                               
                               Configure Folder
                               DONE

                               """;
        
        Assert.Throws<VigoParseFolderConfigException>(() => FolderConfigReader.Parse(content));
    }
    
    [Fact]
    public void RunSomeTestData()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                               
                                #  vîgô some other text
                                
                               # comments and empty lines will be ignored
                                        # indented comment

                               """;
        
        var folderConfig = FolderConfigReader.Parse(content);
        
        
        Assert.Null(folderConfig.KeepEmptyFolder);
        Assert.Null(folderConfig.LocalDefaults);
        Assert.Empty(folderConfig.PartialRules);
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
    }
    
    // ReSharper disable once NotAccessedField.Local
    private readonly ITestOutputHelper _testOutputHelper;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly LoggingLevelSwitch _logLevelSwitch;

    #endregion
}