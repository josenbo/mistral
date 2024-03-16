namespace vigobase_tests;

public class NamedTagTests
{
    [Fact]
    public void LettersAreAccepted()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string theName = "mio";
        var tagName = new NamedTag(theName);
        
        Assert.Equal(theName, tagName.ToString());
    }

    [Fact]
    public void DigitsAndLettersAreAccepted()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string theName = "m3x";
        var tagName = new NamedTag(theName);
        
        Assert.Equal(theName, tagName.ToString());
    }

    [Fact]
    public void DividersDigitsAndLettersAreAccepted()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string theName = "m.3";
        var tagName = new NamedTag(theName);
        
        Assert.Equal(theName, tagName.ToString());
    }

    [Fact]
    public void ComplexNamesAreAccepted()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string theName = "mUnGO-3M.3_w-Tre_tre-12345";
        var tagName = new NamedTag(theName);
        
        Assert.Equal(theName, tagName.ToString());
    }
    
    [Fact]
    public void CaseDoesNotMatter()
    {
        var tagName1 = new NamedTag("bingo");
        var tagName2 = new NamedTag("Bingo");
        var tagName3 = new NamedTag("BINGO");

        Assert.Equal(tagName1, tagName2);
        Assert.Equal(tagName1, tagName3);
        Assert.Equal(tagName2, tagName3);
    }
    
    [Fact]
    public void DifferentNamesAreDifferentTags()
    {
        var tagName1 = new NamedTag("bingo");
        var tagName2 = new NamedTag("bongo");

        Assert.NotEqual(tagName1, tagName2);
    }

    [Fact]
    public void TooShortNamesFail()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        Assert.Throws<ArgumentException>(() => new NamedTag("s"));
    }
    
    [Fact]
    public void ToLongNamesFail()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        Assert.Throws<ArgumentException>(() => new NamedTag("ThisTagNameIsDefinitelyTooLongToBeAccepted"));
    }

    [Fact]
    public void LeadingSpacesFail()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        Assert.Throws<ArgumentException>(() => new NamedTag(" SomeTag"));
    }
    
    [Fact]
    public void TrailingSpacesFail()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        Assert.Throws<ArgumentException>(() => new NamedTag("SomeTag "));
    }
    
    [Fact]
    public void NonAsciiLettersFail()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        Assert.Throws<ArgumentException>(() => new NamedTag("Übel"));
    }

    [Fact]
    public void LeadingDigitFail()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        Assert.Throws<ArgumentException>(() => new NamedTag("3Letters"));
    }
    
    [Fact]
    public void LeadingDividerFail()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        Assert.Throws<ArgumentException>(() => new NamedTag("-Letters"));
    }

    [Fact]
    public void DoubleDividersFail()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        Assert.Throws<ArgumentException>(() => new NamedTag("Prefix_-Suffix"));
    }
    
    #region Helpers

    public NamedTagTests(ITestOutputHelper testOutputHelper)
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