namespace vigoscope_tests;

public class NamedParserTests
{
    /// <summary>
    /// "file~1~UAT~~" does not match, because keywords are missing.
    /// </summary>
    [Fact]
    public void FileNameWithoutTagsIsPassingThrough()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~UAT~~";
        const string targetName = "file~1~UAT~~";
        var parser = CreateNameParser("UAT");

        var parseResult = parser.Parse(sourceName);
        
        Assert.True(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Empty(parseResult.Tags);
    }
    
    /// <summary>
    /// "file~1~DEPLOY~ONLY~UaT~~" yields UaT. Env "uat" is in scope. New file name is "file~1".
    /// </summary>
    [Fact]
    public void DeployOnlyTagRecognizedAndEnvironmentInScope()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~DEPLOY~ONLY~UaT~~";
        const string targetName = "file~1";
        var parser = CreateNameParser("uat");

        var parseResult = parser.Parse(sourceName);
        
        Assert.True(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Empty(parseResult.Tags);
    }

    /// <summary>
    /// "file~1~DEPLOY~ONLY~UaT~~" yields UaT. Env "development" is out of scope.
    /// </summary>
    [Fact]
    public void DeployOnlyTagRecognizedAndEnvironmentOutOfScope()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~DEPLOY~ONLY~UaT~~";
        const string targetName = "";
        var parser = CreateNameParser("development");

        var parseResult = parser.Parse(sourceName);
        
        Assert.False(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Empty(parseResult.Tags);
    }

    /// <summary>
    /// "file~1~DEPLOY~ONLY~Non-Prod~TCB~EXCEPT~UAT~~" yields ["Non-Prod", "TCB"] except ["UAT"].
    /// Env "tst" is in scope. New file name is "file~1".
    /// </summary>
    [Fact]
    public void DeployOnlyExceptTagsRecognizedAndEnvironmentInScope()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~DEPLOY~ONLY~Non-Prod~TCB~EXCEPT~UAT~~";
        const string targetName = "file~1";
        var parser = CreateNameParser("tst", "test", "testing", "#non-prod");

        var parseResult = parser.Parse(sourceName);
        
        Assert.True(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Empty(parseResult.Tags);
    }
    
    /// <summary>
    /// "file~1~DEPLOY~ONLY~Non-Prod~TCB~EXCEPT~UAT~~" yields ["Non-Prod", "TCB"] except ["UAT"].
    /// Env "tst" with group "tcb" fits the first condition, but the alias "uat"
    /// matches the except clause and puts the file out of scope.
    /// </summary>
    [Fact]
    public void DeployOnlyExceptTagsRecognizedAndEnvironmentOutOfScope()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~DEPLOY~ONLY~Non-Prod~TCB~EXCEPT~UAT~~";
        const string targetName = "";
        var parser = CreateNameParser("tst", "uat", "#tcb");

        var parseResult = parser.Parse(sourceName);
        
        Assert.False(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Empty(parseResult.Tags);
    }

    /// <summary>
    /// "file~1~SKIP~DEPLOY~uAt~~" yields uAt. Env "development" is in scope. New name is "file~1" 
    /// </summary>
    [Fact]
    public void SkipDeployTagRecognizedAndEnvironmentInScope()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~SKIP~DEPLOY~uAt~~";
        const string targetName = "file~1";
        var parser = CreateNameParser("development");

        var parseResult = parser.Parse(sourceName);
        
        Assert.True(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Empty(parseResult.Tags);
    }
    
    /// <summary>
    /// "file~1~SKIP~DEPLOY~uAt~~" yields uAt and (empty). Env uat is out of scope.
    /// </summary>
    [Fact]
    public void SkipDeployTagRecognizedAndEnvironmentOutOfScope()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~SKIP~DEPLOY~uAt~~";
        const string targetName = "";
        var parser = CreateNameParser("uat");

        var parseResult = parser.Parse(sourceName);
        
        Assert.False(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Empty(parseResult.Tags);
    }

    /// <summary>
    /// "file~1~SKIP~DEPLOY~ALL~EXCEPT~uAT~~" yields ["ALL"] except ["uAT"].
    /// Env "uat" with group "all" is in scope. New name is "file~1" 
    /// </summary>
    [Fact]
    public void SkipDeployExceptTagsRecognizedAndEnvironmentInScope()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~SKIP~DEPLOY~ALL~EXCEPT~uAT~~";
        const string targetName = "file~1";
        var parser = CreateNameParser("uat", "#all");

        var parseResult = parser.Parse(sourceName);
        
        Assert.True(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Empty(parseResult.Tags);
    }

    /// <summary>
    /// "file~1~SKIP~DEPLOY~ALL~EXCEPT~uAT~~" yields ["ALL"] except ["uAT"].
    /// Env "dev" with group "all" is out of scope. 
    /// </summary>
    [Fact]
    public void SkipDeployExceptTagsRecognizedAndEnvironmentOutOfScope()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~SKIP~DEPLOY~ALL~EXCEPT~uAT~~";
        const string targetName = "";
        var parser = CreateNameParser("dev", "#all");

        var parseResult = parser.Parse(sourceName);
        
        Assert.False(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Empty(parseResult.Tags);
    }
    
    /// <summary>
    /// "file~1~TAGS~one~two.2~three-3~four_4~~" yields ["one", "two.2", "three-3", "four_4"].
    /// The file is in scope. The new name is "file~1".  
    /// </summary>
    [Fact]
    public void SimpleTagListIsExtractedAndFileRenamed()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~TAGS~one~two.2~three-3~four_4~~";
        const string targetName = "file~1";
        var parser = CreateNameParser("dev", "#all");

        var parseResult = parser.Parse(sourceName);
        
        Assert.True(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Equal(4, parseResult.Tags.Count());
        Assert.Contains(new NamedTag("one"), parseResult.Tags);
        Assert.Contains(new NamedTag("two.2"), parseResult.Tags);
        Assert.Contains(new NamedTag("three-3"), parseResult.Tags);
        Assert.Contains(new NamedTag("four_4"), parseResult.Tags);
    }

    /// <summary>
    /// "file~1~TAGS~o.1~w_3556~DEPLOY~ONLY~tcb~EXCEPT~dev~~" yields ["o.1", "w_3556"].
    /// The file is in scope. The new name is "file~1".  
    /// </summary>
    [Fact]
    public void LeadingTagListWithDeployOnlyExceptIsExtractedAndFileRenamed()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~TAGS~o.1~w_3556~DEPLOY~ONLY~tcb~EXCEPT~dev~~";
        const string targetName = "file~1";
        var parser = CreateNameParser("uat", "#tcb");

        var parseResult = parser.Parse(sourceName);
        
        Assert.True(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Equal(2, parseResult.Tags.Count());
        Assert.Contains(new NamedTag("o.1"), parseResult.Tags);
        Assert.Contains(new NamedTag("w_3556"), parseResult.Tags);
    }

    /// <summary>
    /// "file~1~SKIP~DEPLOY~tcb~EXCEPT~uat~TAGS~o.1~w_3556~~" yields ["o.1", "w_3556"].
    /// The file is in scope. The new name is "file~1".  
    /// </summary>
    [Fact]
    public void TrailingTagListWithSkipDeployExceptIsExtractedAndFileRenamed()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~SKIP~DEPLOY~tcb~EXCEPT~uat~TAGS~o.1~w_3556~~";
        const string targetName = "file~1";
        var parser = CreateNameParser("uat", "#tcb");

        var parseResult = parser.Parse(sourceName);
        
        Assert.True(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Equal(2, parseResult.Tags.Count());
        Assert.Contains(new NamedTag("o.1"), parseResult.Tags);
        Assert.Contains(new NamedTag("w_3556"), parseResult.Tags);
    }

    /// <summary>
    /// "file~1~TAGS~top~DEPLOY~ONLY~DEV~~" yields tags ["top"].
    /// The file is out of scope, so tags are removed.
    /// </summary>
    [Fact]
    public void OutOfScopeFileWithTagsHasTagsRemoved()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~TAGS~top~DEPLOY~ONLY~DEV~~";
        const string targetName = "";
        var parser = CreateNameParser("uat", "#tcb");

        var parseResult = parser.Parse(sourceName);
        
        Assert.False(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Empty(parseResult.Tags);
    }
    
    /// <summary>
    /// "file~1~uat~dev~test~~" is close, but does not match. It is not yet considered an error
    /// </summary>
    [Fact]
    public void NonMatchingFileNameWithThreeLikelyTagsIsPassingThrough()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~uat~dev~test~~";
        const string targetName = "file~1~uat~dev~test~~";
        var parser = CreateNameParser("UAT");

        var parseResult = parser.Parse(sourceName);
        
        Assert.True(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Empty(parseResult.Tags);
    }

    /// <summary>
    /// "file~1~uat~dev~test~ref~~" is close, but does not match.
    ///  This is considered an error, because there are more than
    ///  three potential tags
    /// </summary>
    [Fact]
    public void NonMatchingFileNameWithFourLikelyTagsThrowsAnException()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~uat~dev~test~ref~~";
        var parser = CreateNameParser("UAT");

        Assert.Throws<NameParserException>(() => parser.Parse(sourceName));
    }

    /// <summary>
    /// "file~1~one~tags~~" does not match, but resembles to closely
    ///  a valid match, because it contains the keyword "tags" 
    /// </summary>
    [Fact]
    public void NonMatchingFileNameWithKeywordThrowsAnException()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~one~tags~~";
        var parser = CreateNameParser("UAT");

        Assert.Throws<NameParserException>(() => parser.Parse(sourceName));
    }

    /// <summary>
    /// "file~1~Deploy~onLY~Non-Prod~TCB~except~UAT~~" matches using mixed case keywords.
    /// Env "tst" is in scope. New file name is "file~1".
    /// </summary>
    [Fact]
    public void MixedCaseDeployOnlyExceptTagsRecognizedAndEnvironmentInScope()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string sourceName = "file~1~Deploy~onLY~Non-Prod~TCB~except~UAT~~";
        const string targetName = "file~1";
        var parser = CreateNameParser("tst", "test", "testing", "#non-prod");

        var parseResult = parser.Parse(sourceName);
        
        Assert.True(parseResult.CanDeploy);
        Assert.Equal(sourceName, parseResult.SourceName);
        Assert.Equal(targetName, parseResult.TargetName);
        Assert.Empty(parseResult.Tags);
    }

    #region Helpers

    private static IEnumerable<string> StringList(params string[] tags)
    {
        return tags.ToList();
    }

    private INameParser CreateNameParser(params string[] tagNames)
    {
        var envBuilder = new EnvironmentDescriptorBuilder(tagNames[0]);

        for (var i = 1; i < tagNames.Length; i++)
        {
            if (tagNames[i].StartsWith('#'))
                envBuilder.AddGroup(tagNames[i][1..]);
            else
                envBuilder.AddAlias(tagNames[i]);
        }
        
        return NameParserFactory.Create(envBuilder.Build());
    }
    
    public NamedParserTests(ITestOutputHelper testOutputHelper)
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