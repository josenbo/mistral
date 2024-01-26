using System.Text.RegularExpressions;
// ReSharper disable StringLiteralTypo

namespace vigobase_tests;

public partial class PermissionUnixFileModeTests
{
    [Fact]
    public void GrantExecuteToAll1()
    {
        const string symbolicNotation = "a+x";
        var defaultUnixFileMode = GetUnixFileMode("r---w-rw-");
        var expectedUnixFileMode = GetUnixFileMode("r-x-wxrwx");

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }

    [Fact]
    public void GrantExecuteToAll2()
    {
        const string symbolicNotation = "+x";
        var defaultUnixFileMode = GetUnixFileMode("r---w-rw-");
        var expectedUnixFileMode = GetUnixFileMode("r-x-wxrwx");

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }

    [Fact]
    public void GrantExecuteToAll3()
    {
        const string symbolicNotation = "ugo+x";
        var defaultUnixFileMode = GetUnixFileMode("r---w-rw-");
        var expectedUnixFileMode = GetUnixFileMode("r-x-wxrwx");

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }

    [Fact]
    public void GrantExecuteToAll4()
    {
        const string symbolicNotation = "u+x,g+x,o+x";
        var defaultUnixFileMode = GetUnixFileMode("r---w-rw-");
        var expectedUnixFileMode = GetUnixFileMode("r-x-wxrwx");

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }

    [Fact]
    public void GrantExecuteToAll5()
    {
        const string symbolicNotation = "o+x,gu+x";
        var defaultUnixFileMode = GetUnixFileMode("r---w-rw-");
        var expectedUnixFileMode = GetUnixFileMode("r-x-wxrwx");

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }
    
    [Fact]
    public void GrantReadWriteToGroup()
    {
        const string symbolicNotation = "g+rw";
        var defaultUnixFileMode = GetUnixFileMode("rwx-w--w-");
        var expectedUnixFileMode = GetUnixFileMode("rwxrw--w-");

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }

    [Fact]
    public void SetUserToReadExecute1()
    {
        const string symbolicNotation = "u=rx";
        var defaultUnixFileMode = GetUnixFileMode("-wx-w--w-");
        var expectedUnixFileMode = GetUnixFileMode("r-x-w--w-");

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }

    [Fact]
    public void SetUserToReadExecute2()
    {
        const string symbolicNotation = "u=rw,u-w,u+x";
        var defaultUnixFileMode = GetUnixFileMode("-wx-w--w-");
        var expectedUnixFileMode = GetUnixFileMode("r-x-w--w-");

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }

    [Fact]
    public void SetAllToNone1()
    {
        const string symbolicNotation = "=";
        var defaultUnixFileMode = GetUnixFileMode("-wx-w--w-");
        var expectedUnixFileMode = UnixFileMode.None;

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }
    
    [Fact]
    public void SetAllToNone2()
    {
        const string symbolicNotation = "a=";
        var defaultUnixFileMode = GetUnixFileMode("-wx-w--w-");
        var expectedUnixFileMode = UnixFileMode.None;

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }
    
    [Fact]
    public void SetAllToNone3()
    {
        const string symbolicNotation = "ogu=";
        var defaultUnixFileMode = GetUnixFileMode("-wx-w--w-");
        var expectedUnixFileMode = UnixFileMode.None;

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }
    
    [Fact]
    public void SetAllToNone4()
    {
        const string symbolicNotation = "u-wx,go-w";
        var defaultUnixFileMode = GetUnixFileMode("-wx-w--w-");
        var expectedUnixFileMode = UnixFileMode.None;

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }
    
    [Fact]
    public void SetAllToNone5()
    {
        const string symbolicNotation = "ogu-wx";
        var defaultUnixFileMode = GetUnixFileMode("-wx-w--w-");
        var expectedUnixFileMode = UnixFileMode.None;

        var symbolicPermission = FilePermission.SpecifySymbolic(symbolicNotation);
        var effectiveFileMode = symbolicPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }

    [Fact]
    public void CheckDefaultPermission()
    {
        var defaultUnixFileMode = GetUnixFileMode("-wx-w--w-");
        var expectedUnixFileMode = defaultUnixFileMode;

        var defaultPermission = FilePermission.UseDefault;
        var effectiveFileMode = defaultPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }

    [Fact]
    public void CheckOctalPermission()
    {
        const string octalNotation = "724";
        var defaultUnixFileMode = GetUnixFileMode("---rwx---");
        var expectedUnixFileMode = GetUnixFileMode("rwx-w-r--");

        var octalPermission = FilePermission.SpecifyOctal(octalNotation);
        var effectiveFileMode = octalPermission.ComputeUnixFileMode(defaultUnixFileMode);

        Assert.Equal(expectedUnixFileMode, effectiveFileMode);
    }
    
    #region Helpers

    private static UnixFileMode GetUnixFileMode(string flags)
    {
        if (!FileModeFlags.IsMatch(flags))
            throw new ArgumentException($"Invalid FileModeFlags {flags}", nameof(flags));
        
        var retval = UnixFileMode.None;

        if (flags[0] == 'r') retval |= UnixFileMode.UserRead;
        if (flags[1] == 'w') retval |= UnixFileMode.UserWrite;
        if (flags[2] == 'x') retval |= UnixFileMode.UserExecute;
        if (flags[3] == 'r') retval |= UnixFileMode.GroupRead;
        if (flags[4] == 'w') retval |= UnixFileMode.GroupWrite;
        if (flags[5] == 'x') retval |= UnixFileMode.GroupExecute;
        if (flags[6] == 'r') retval |= UnixFileMode.OtherRead;
        if (flags[7] == 'w') retval |= UnixFileMode.OtherWrite;
        if (flags[8] == 'x') retval |= UnixFileMode.OtherExecute;
        return retval;
    }
    
    public PermissionUnixFileModeTests(ITestOutputHelper testOutputHelper)
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

    private static readonly Regex FileModeFlags = CompiledRegexFileModeFlags();
    // ReSharper disable once NotAccessedField.Local
    private readonly ITestOutputHelper _testOutputHelper;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly LoggingLevelSwitch _logLevelSwitch;

    [GeneratedRegex("^[r-][w-][x-][r-][w-][x-][r-][w-][x-]$", RegexOptions.None)]
    private static partial Regex CompiledRegexFileModeFlags();

    #endregion
}