// ReSharper disable StringLiteralTypo
namespace vigoconfig_tests;

public class ApiTests
{
    [Fact]
    public void FolderAndOneRule()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        var appSettings = new TestAppSettings();
        
        var success = FolderConfigurationApi.Reader.TryParse(
            configurationScript: """
                                 CONFIGURE FOLDER
                                     DEFAULT BUILD TARGETS NONE
                                     DEFAULT FOR SOURCE ENCODING ISO-8859-15
                                 DONE
                                 
                                 DO DEPLOY BINARY FILE IF NAME EQUALS sample.bin
                                     RENAME TO babwire
                                     VALID CHARACTERS Ascii
                                     SOURCE ENCODING Ascii
                                 DONE
                                 """,
            configurationFile: $"{nameof(ApiTests)}.{nameof(FolderAndOneRule)}",
            configurationType: ConfigurationFileTypeEnum.NativeFormat,
            initialDefaults: appSettings.DefaultFileHandlingParams,
            folderConfiguration: out var folderConfiguration);

        Assert.True(success);
        Assert.NotNull(folderConfiguration);
        Assert.Empty(folderConfiguration.FolderDefaults.Targets);
        Assert.Equal(FileEncodingEnum.ISO_8859_15, folderConfiguration.FolderDefaults.SourceFileEncoding);
        Assert.Single(folderConfiguration.RuleConfigurations);
        Assert.Equal(FileRuleActionEnum.DeployFile, folderConfiguration.RuleConfigurations.First().Action);
        Assert.Equal(FileRuleConditionEnum.MatchName, folderConfiguration.RuleConfigurations.First().Condition);
        Assert.Equal("sample.bin", folderConfiguration.RuleConfigurations.First().CompareWith);
        Assert.Equal("babwire", folderConfiguration.RuleConfigurations.First().ReplaceWith);
        Assert.Equal(FileEncodingEnum.Ascii, folderConfiguration.RuleConfigurations.First().Handling.SourceFileEncoding);
        Assert.Equal(FilePermission.UseDefault, folderConfiguration.RuleConfigurations.First().Handling.Permissions);
    }
    
    #region Helpers

    public ApiTests(ITestOutputHelper testOutputHelper)
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