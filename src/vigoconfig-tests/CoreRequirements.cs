using System.Diagnostics.CodeAnalysis;

namespace vigoconfig_tests;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
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

                               # comments and empty lines will be ignored
                               # comment lines begin with a hash as the
                               # first non-whitespace character 
                               
                               # DO IGNORE TEXT FILE
                                # DONE
                               
                                     #  CONFIGURE FOLDER
                                     #      doing folder 
                                     #      configuration
                                     #  DONE
                               
                               #do deploy text file 
                               #    if name is hubert
                               #    do something with hubert
                               #done
                               
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
    public void SolitaryDefaultFileMode()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                                
                               configure folder
                                   default for file mode = 600
                               done

                               # vîgô
                               """;

        var folderConfig = FolderConfigReader.Parse(content);
        
        Assert.NotNull(folderConfig);
        Assert.Null(folderConfig.KeepEmptyFolder);
        Assert.Empty(folderConfig.PartialRules);
        Assert.NotNull(folderConfig.LocalDefaults);
        Assert.Null(folderConfig.LocalDefaults.FileType);
        Assert.NotNull(folderConfig.LocalDefaults.StandardModeForFiles);
        Assert.True(folderConfig.LocalDefaults.StandardModeForFiles == (UnixFileMode)0b_110_000_000);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForDirectories);
        Assert.Null(folderConfig.LocalDefaults.Permissions);
        Assert.Null(folderConfig.LocalDefaults.SourceFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.TargetFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.LineEnding);
        Assert.Null(folderConfig.LocalDefaults.FixTrailingNewline);
        Assert.False(folderConfig.LocalDefaults.IsDefinedValidCharsRegex);
        Assert.Null(folderConfig.LocalDefaults.Targets);
    }
    
    [Fact]
    public void SolitaryDefaultSourceEncoding()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                                
                               configure folder
                                   DEFAULT for SoUrCe encoding       = win_1252
                               done

                               # vîgô
                               """;

        var folderConfig = FolderConfigReader.Parse(content);
        
        Assert.NotNull(folderConfig);
        Assert.Null(folderConfig.KeepEmptyFolder);
        Assert.Empty(folderConfig.PartialRules);
        Assert.NotNull(folderConfig.LocalDefaults);
        Assert.Null(folderConfig.LocalDefaults.FileType);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForFiles);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForDirectories);
        Assert.Null(folderConfig.LocalDefaults.Permissions);
        Assert.NotNull(folderConfig.LocalDefaults.SourceFileEncoding);
        Assert.Equal(FileEncodingEnum.Windows_1252, folderConfig.LocalDefaults.SourceFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.TargetFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.LineEnding);
        Assert.Null(folderConfig.LocalDefaults.FixTrailingNewline);
        Assert.False(folderConfig.LocalDefaults.IsDefinedValidCharsRegex);
        Assert.Null(folderConfig.LocalDefaults.Targets);
    }
    
    [Fact]
    public void SolitaryDefaultTargetEncoding()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                                
                               configure folder
                                   DEFAULT for tARget encoding       = iso-88_5.9 1
                               done

                               # vîgô
                               """;

        var folderConfig = FolderConfigReader.Parse(content);
        
        Assert.NotNull(folderConfig);
        Assert.Null(folderConfig.KeepEmptyFolder);
        Assert.Empty(folderConfig.PartialRules);
        Assert.NotNull(folderConfig.LocalDefaults);
        Assert.Null(folderConfig.LocalDefaults.FileType);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForFiles);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForDirectories);
        Assert.Null(folderConfig.LocalDefaults.Permissions);
        Assert.Null(folderConfig.LocalDefaults.SourceFileEncoding);
        Assert.NotNull(folderConfig.LocalDefaults.TargetFileEncoding);
        Assert.Equal(FileEncodingEnum.ISO_8859_1, folderConfig.LocalDefaults.TargetFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.LineEnding);
        Assert.Null(folderConfig.LocalDefaults.FixTrailingNewline);
        Assert.False(folderConfig.LocalDefaults.IsDefinedValidCharsRegex);
        Assert.Null(folderConfig.LocalDefaults.Targets);
    }

    [Fact]
    public void SolitaryDefaultNewline()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                                
                               configure folder
                                   DEFAULT 
                                     for 
                                       newline       
                                         = linux
                               done

                               # vîgô
                               """;

        var folderConfig = FolderConfigReader.Parse(content);
        
        Assert.NotNull(folderConfig);
        Assert.Null(folderConfig.KeepEmptyFolder);
        Assert.Empty(folderConfig.PartialRules);
        Assert.NotNull(folderConfig.LocalDefaults);
        Assert.Null(folderConfig.LocalDefaults.FileType);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForFiles);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForDirectories);
        Assert.Null(folderConfig.LocalDefaults.Permissions);
        Assert.Null(folderConfig.LocalDefaults.SourceFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.TargetFileEncoding);
        Assert.NotNull(folderConfig.LocalDefaults.LineEnding);
        Assert.Equal(LineEndingEnum.LF, folderConfig.LocalDefaults.LineEnding);
        Assert.Null(folderConfig.LocalDefaults.FixTrailingNewline);
        Assert.False(folderConfig.LocalDefaults.IsDefinedValidCharsRegex);
        Assert.Null(folderConfig.LocalDefaults.Targets);
    }

    [Fact]
    public void SolitaryDefaultAddTrailingNewline()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                                
                               configure folder
                                   DEFAULT for add
                               trailing
                                 newline = true
                               done

                               # vîgô
                               """;

        var folderConfig = FolderConfigReader.Parse(content);
        
        Assert.NotNull(folderConfig);
        Assert.Null(folderConfig.KeepEmptyFolder);
        Assert.Empty(folderConfig.PartialRules);
        Assert.NotNull(folderConfig.LocalDefaults);
        Assert.Null(folderConfig.LocalDefaults.FileType);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForFiles);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForDirectories);
        Assert.Null(folderConfig.LocalDefaults.Permissions);
        Assert.Null(folderConfig.LocalDefaults.SourceFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.TargetFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.LineEnding);
        Assert.NotNull(folderConfig.LocalDefaults.FixTrailingNewline);
        Assert.True(folderConfig.LocalDefaults.FixTrailingNewline);
        Assert.False(folderConfig.LocalDefaults.IsDefinedValidCharsRegex);
        Assert.Null(folderConfig.LocalDefaults.Targets);
    }

    [Fact]
    public void SolitaryDefaultValidCharactersAsciiGermanPlus()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                                
                               configure folder
                                 DEFAULT FOR VALID CHARACTERS    =               AsciiGerman + îô
                               done

                               # vîgô
                               """;

        var folderConfig = FolderConfigReader.Parse(content);
        
        Assert.NotNull(folderConfig);
        Assert.Null(folderConfig.KeepEmptyFolder);
        Assert.Empty(folderConfig.PartialRules);
        Assert.NotNull(folderConfig.LocalDefaults);
        Assert.Null(folderConfig.LocalDefaults.FileType);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForFiles);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForDirectories);
        Assert.Null(folderConfig.LocalDefaults.Permissions);
        Assert.Null(folderConfig.LocalDefaults.SourceFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.TargetFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.LineEnding);
        Assert.Null(folderConfig.LocalDefaults.FixTrailingNewline);
        Assert.True(folderConfig.LocalDefaults.IsDefinedValidCharsRegex);
        Assert.NotNull(folderConfig.LocalDefaults.ValidCharsRegex);
        Assert.Equal("^[\\u0000-\\u007FäöüÄÖÜß€îô]*$", folderConfig.LocalDefaults.ValidCharsRegex.ToString());
        Assert.Null(folderConfig.LocalDefaults.Targets);
    }

    [Fact]
    public void SolitaryDefaultValidCharactersAll()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                                
                               configure folder
                                 DEFAULT FOR VALID CHARACTERS=all
                               done

                               # vîgô
                               """;

        var folderConfig = FolderConfigReader.Parse(content);
        
        Assert.NotNull(folderConfig);
        Assert.Null(folderConfig.KeepEmptyFolder);
        Assert.Empty(folderConfig.PartialRules);
        Assert.NotNull(folderConfig.LocalDefaults);
        Assert.Null(folderConfig.LocalDefaults.FileType);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForFiles);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForDirectories);
        Assert.Null(folderConfig.LocalDefaults.Permissions);
        Assert.Null(folderConfig.LocalDefaults.SourceFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.TargetFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.LineEnding);
        Assert.Null(folderConfig.LocalDefaults.FixTrailingNewline);
        Assert.True(folderConfig.LocalDefaults.IsDefinedValidCharsRegex);
        Assert.Null(folderConfig.LocalDefaults.ValidCharsRegex);
        Assert.Null(folderConfig.LocalDefaults.Targets);
    }

    [Fact]
    public void SolitaryDefaultBuildTargets()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string content = """
                               #!/usr/bin/env vigo
                                
                               configure folder
                                   DEFAULT BUILD TARGETS = one, two; three four five-5
                               done

                               # vîgô
                               """;

        var folderConfig = FolderConfigReader.Parse(content);
        
        Assert.NotNull(folderConfig);
        Assert.Null(folderConfig.KeepEmptyFolder);
        Assert.Empty(folderConfig.PartialRules);
        Assert.NotNull(folderConfig.LocalDefaults);
        Assert.Null(folderConfig.LocalDefaults.FileType);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForFiles);
        Assert.Null(folderConfig.LocalDefaults.StandardModeForDirectories);
        Assert.Null(folderConfig.LocalDefaults.Permissions);
        Assert.Null(folderConfig.LocalDefaults.SourceFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.TargetFileEncoding);
        Assert.Null(folderConfig.LocalDefaults.LineEnding);
        Assert.Null(folderConfig.LocalDefaults.FixTrailingNewline);
        Assert.False(folderConfig.LocalDefaults.IsDefinedValidCharsRegex);
        Assert.NotNull(folderConfig.LocalDefaults.Targets);
        Assert.NotEmpty(folderConfig.LocalDefaults.Targets);
        Assert.Equal(5, folderConfig.LocalDefaults.Targets.Count);
        Assert.Contains("one", folderConfig.LocalDefaults.Targets);
        Assert.Contains("two", folderConfig.LocalDefaults.Targets);
        Assert.Contains("three", folderConfig.LocalDefaults.Targets);
        Assert.Contains("four", folderConfig.LocalDefaults.Targets);
        Assert.Contains("five-5", folderConfig.LocalDefaults.Targets);
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

                               CONFIGURE FOLDER
                                   DEFAULT FOR FILE MODE = 755
                                   DEFAULT FOR SOURCE ENCODING = UTF-8
                                   DEFAULT FOR TARGET ENCODING = UTF-8
                               DONE
                               
                               DO DEPLOY TEXT FILE IF NAME PATTERN = weirdo[0-9]{1,3}.zippo-dong
                                   RENAME TO = abowitz
                                   NAME REPLACE PATTERN = sino123
                                   FILE MODE = 755
                                   SOURCE ENCODING = UTF-8
                                   TARGET ENCODING = UTF-8
                                   NEWLINE = LINUX 
                                   ADD TRAILING NEWLINE = true
                                   VALID CHARACTERS = AsciiGerman
                                   BUILD TARGETS = a, b ,c
                               DONE
                               
                               """;
        
        var folderConfig = FolderConfigReader.Parse(content);
        
        
        Assert.Null(folderConfig.KeepEmptyFolder);
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