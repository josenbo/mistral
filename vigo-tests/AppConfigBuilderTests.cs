using vigo;

namespace vigo_tests;

public class AppConfigBuilderTests
{
    // [Fact]
    // public void Test1()
    // {
    //     _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
    //
    //     var result = AppConfigBuilder.Assemble(Array.Empty<string>());
    // }
    
    #region Helpers

    public AppConfigBuilderTests(ITestOutputHelper testOutputHelper)
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