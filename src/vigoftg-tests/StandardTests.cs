namespace vigoftg_tests;

public class StandardTests
{
    [Fact]
    public void FileNameWithoutTagsIsPassingThrough()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file~1~UAT~~";
        var parser = new NameParser();
        AppendCaseInsensitiveTags(parser, "UAT", "REF", "PROD");
        var activeTags = StringList("UAT");

        var parseResult = parser.ParseFileName(fileName, activeTags);
        
        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.False(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal(string.Empty, parseResult.NewName);
    }
    
    [Fact]
    public void TagIsMatchedWhenValidAndActiveAndCaseDoesNotMatter()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file~1~DEPLOY~ONLY~UaT~~";
        var parser = new NameParser();
        AppendCaseInsensitiveTags(parser, "uat", "ref", "prod");
        var activeTags = StringList("UAT");

        var parseResult = parser.ParseFileName(fileName, activeTags);
        
        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.True(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal("file~1", parseResult.NewName);
    }

    /// <summary>
    /// file~1~DEPLOY~ONLY~uaT~~ -> OK, rename to file~1, env uaT matched, no tags
    /// </summary>
    [Fact]
    public void TagIsMatchedWhenValidAndActiveAndCaseMatches()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file~1~DEPLOY~ONLY~uaT~~";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "uaT", "Uat", "uat", "UAT");
        var activeTags = StringList("uaT");

        var parseResult = parser.ParseFileName(fileName, activeTags);
        
        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.True(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal("file~1", parseResult.NewName);
    }

    [Fact]
    public void ParsingFailsWhenTagNotFoundBecauseCaseDiffersInFileName()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file~1~DEPLOY~ONLY~uAT~~";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "uaT", "Uat", "uat", "UAT");
        var activeTags = StringList("uaT");

        var parseResult = parser.ParseFileName(fileName, activeTags);
        
        Assert.False(parseResult.Success);
        Assert.True(parseResult.DoIgnore);
        Assert.False(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal(string.Empty, parseResult.NewName);
    }
    
    [Fact]
    public void ParsingFailsWhenTagNotFoundBecauseCaseDiffersInActiveTags()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file~1~DEPLOY~ONLY~uaT~~";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "uaT", "Uat", "uat", "UAT");
        var activeTags = StringList("UAt");

        var parseResult = parser.ParseFileName(fileName, activeTags);
        
        Assert.True(parseResult.Success);
        Assert.True(parseResult.DoIgnore);
        Assert.False(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal(string.Empty, parseResult.NewName);
    }
    
    [Fact]
    public void MisspelledDeployOnlyKeywordIsIgnored()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file~1~DEPLoY~ONLY~a1~~";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "a1", "b2");
        var activeTags = StringList("a1");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.False(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal(string.Empty, parseResult.NewName);
    }

    /// <summary>
    /// file-1-DEPLOY-ONLY-a1-- -> OK, rename to file-1, env a1 matched, no tags
    /// </summary>
    [Fact]
    public void DeployOnlyWithHyphenAndSingleTagIsTransformed()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file-1-DEPLOY-ONLY-a1--";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "a1", "b2");
        var activeTags = StringList("a1");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.True(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal("file-1", parseResult.NewName);
    }

    /// <summary>
    /// file_1_DEPLOY_ONLY_a1__ -> OK, rename to file_1, env a1 matched, no tags 
    /// </summary>
    [Fact]
    public void DeployOnlyWithUnderscoreAndSingleTagIsTransformed()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file_1_DEPLOY_ONLY_a1__";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "a1", "b2");
        var activeTags = StringList("a1");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.True(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal("file_1", parseResult.NewName);
    }

    /// <summary>
    /// file.1.DEPLOY.ONLY.a1.. -> OK, rename to file.1, env a1 matched, no tags   
    /// </summary>
    [Fact]
    public void DeployOnlyWithDotAndSingleTagIsTransformed()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file.1.DEPLOY.ONLY.a1..";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "a1", "b2");
        var activeTags = StringList("a1");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.True(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal("file.1", parseResult.NewName);
    }

    [Fact]
    public void DeployOnlyWithPlusIsNotAScopeTag()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file+1+DEPLOY+ONLY+a1++";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "a1", "b2");
        var activeTags = StringList("a1");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.False(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal(string.Empty, parseResult.NewName);
    }

    [Fact]
    public void RenameWhenInclusiveTagsMatchButNotTheExceptions()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file.1.DEPLOY.ONLY.a1.EXCEPT.b2..";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "a1", "b2");
        var activeTags = StringList("a1");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.True(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal("file.1", parseResult.NewName);
    }

    [Fact]
    public void DeployOnlyWithUnderscoreThreeTagsAndFourExceptionTagsIsTransformed()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file~1_DEPLOY_ONLY_a1_another-tag-with-delimiters_EXCEPT2_EXCEPT_b2_except_k2_jodeldiplom-dot-tilde-HYPhen__.suffix";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "a1", "another-tag-with-delimiters", "EXCEPT2", "b2", "except", "k2", "jodeldiplom-dot-tilde-HYPhen");
        var activeTags = StringList("a1");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.True(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal("file~1.suffix", parseResult.NewName);
    }

    [Fact]
    public void SkipDeployWithDotAndTwoTagsIsTransformed()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file-name.SKIP.DEPLOY.one.Two...fish";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "one", "Two", "two");
        var activeTags = StringList("two");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.True(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal("file-name.fish", parseResult.NewName);
    }

    [Fact]
    public void SkipDeployOnlyIsNotAScopeTag()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file-name.SKIP.DEPLOY.ONLY.one.Two...fish";
        var parser = new NameParser();
        AppendCaseInsensitiveTags(parser, "one", "two");
        var activeTags = StringList("two");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.False(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal(string.Empty, parseResult.NewName);
    }

    [Fact]
    public void MultiplePatternMatchesWithDifferentDelimitersAreNotScopeTags()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "start-DEPLOY-ONLY-a1--middle~SKIP~DEPLOY~a7~~end";
        var parser = new NameParser();
        AppendCaseInsensitiveTags(parser, "a1", "a7");
        var activeTags = StringList("a7");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.False(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal(string.Empty, parseResult.NewName);
    }

    [Fact]
    public void MultiplePatternMatchesWithSameDelimiterAreNotScopeTags()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "start.DEPLOY.ONLY.a1..middle.SKIP.DEPLOY.a7..end";
        var parser = new NameParser();
        AppendCaseInsensitiveTags(parser, "a1", "a7");
        var activeTags = StringList("a7");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.False(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal(string.Empty, parseResult.NewName);
    }
    
    [Fact]
    public void LeadingAdditionalTagsAreRecognized()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file~1~TAGS~Hans-Dampf~B_3~DEPLOY~ONLY~a1~~x";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "a1", "b2");
        var activeTags = StringList("a1");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.True(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal("file~1x", parseResult.NewName);
        Assert.Contains(parseResult.Tags, s => s == "Hans-Dampf");
        Assert.Contains(parseResult.Tags, s => s == "B_3");
        Assert.True(parseResult.HasTag("Hans-Dampf"));
        Assert.True(parseResult.HasTag("B_3"));
        Assert.False(parseResult.HasTag("a1"));
    }
    
    [Fact]
    public void TrailingAdditionalTagsAreRecognized()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "file~1~DEPLOY~ONLY~a1~TAGS~HansDampf~B_3_2-3_10_x~~";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "a1", "b2");
        var activeTags = StringList("a1");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.True(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal("file~1", parseResult.NewName);
        Assert.Contains(parseResult.Tags, s => s == "HansDampf");
        Assert.Contains(parseResult.Tags, s => s == "B_3_2-3_10_x");
        Assert.True(parseResult.HasTag("HansDampf"));
        Assert.True(parseResult.HasTag("B_3_2-3_10_x"));
        Assert.False(parseResult.HasTag("a1"));
    }

    [Fact]
    public void SolitaryAdditionalTagsAreRecognized()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        const string fileName = "~TAGS~HansDampf~B_3_2-3_10_x~~nur.suffix";
        var parser = new NameParser();
        AppendCaseSensitiveTags(parser, "a1", "b2");
        var activeTags = StringList("a1");

        var parseResult = parser.ParseFileName(fileName, activeTags);

        Assert.True(parseResult.Success);
        Assert.False(parseResult.DoIgnore);
        Assert.True(parseResult.DoRename);
        Assert.Equal(fileName, parseResult.CurrentName);
        Assert.Equal("nur.suffix", parseResult.NewName);
        Assert.Contains(parseResult.Tags, s => s == "HansDampf");
        Assert.Contains(parseResult.Tags, s => s == "B_3_2-3_10_x");
        Assert.True(parseResult.HasTag("HansDampf"));
        Assert.True(parseResult.HasTag("B_3_2-3_10_x"));
        Assert.False(parseResult.HasTag("a1"));
        Assert.False(parseResult.HasTag("b2"));
    }
    
    #region Helpers

    private static void AppendCaseSensitiveTags(NameParser parser, params string[] tags)
    {
        parser.AddCaseSensitiveTags(tags);
    }

    private static void AppendCaseInsensitiveTags(NameParser parser, params string[] tags)
    {
        parser.AddCaseInsensitiveTags(tags);
    }

    // ReSharper disable once UnusedMember.Local
    private static void AppendTags(NameParser parser, bool isCaseSensitive, params string[] tags)
    {
        parser.AddTags(isCaseSensitive, tags);
    }

    private static IEnumerable<string> StringList(params string[] tags)
    {
        return tags.ToList();
    }
    
    public StandardTests(ITestOutputHelper testOutputHelper)
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