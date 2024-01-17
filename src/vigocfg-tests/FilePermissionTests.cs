namespace vigocfg_tests;

public class FilePermissionTests
{
    [Fact]
    public void ClassificationForUndefined()
    {
        const FilePermissionTypeEnum permType = FilePermissionTypeEnum.Undefined;
        const int permTypeInt = (int)permType;
        
        Assert.True(permType.IsDefined());
        Assert.False(permType.IsDefinedAndValid());
        Assert.False(permType.IsOctalNotation());
        Assert.False(permType.IsSymbolicNotation());
        Assert.True(FilePermissionTypeEnumHelper.IsDefined(permTypeInt));
        Assert.False(FilePermissionTypeEnumHelper.IsDefinedAndValid(permTypeInt));
    }
    
    [Fact]
    public void ClassificationForDefault()
    {
        const FilePermissionTypeEnum permType = FilePermissionTypeEnum.Default;
        const int permTypeInt = (int)permType;
        
        Assert.True(permType.IsDefined());
        Assert.True(permType.IsDefinedAndValid());
        Assert.False(permType.IsOctalNotation());
        Assert.False(permType.IsSymbolicNotation());
        Assert.True(FilePermissionTypeEnumHelper.IsDefined(permTypeInt));
        Assert.True(FilePermissionTypeEnumHelper.IsDefinedAndValid(permTypeInt));
    }
    
    [Fact]
    public void ClassificationForOctal()
    {
        const FilePermissionTypeEnum permType = FilePermissionTypeEnum.Octal;
        const int permTypeInt = (int)permType;
        
        Assert.True(permType.IsDefined());
        Assert.True(permType.IsDefinedAndValid());
        Assert.True(permType.IsOctalNotation());
        Assert.False(permType.IsSymbolicNotation());
        Assert.True(FilePermissionTypeEnumHelper.IsDefined(permTypeInt));
        Assert.True(FilePermissionTypeEnumHelper.IsDefinedAndValid(permTypeInt));
    }
    
    [Fact]
    public void ClassificationForSymbolic()
    {
        const FilePermissionTypeEnum permType = FilePermissionTypeEnum.Symbolic;
        const int permTypeInt = (int)permType;
        
        Assert.True(permType.IsDefined());
        Assert.True(permType.IsDefinedAndValid());
        Assert.False(permType.IsOctalNotation());
        Assert.True(permType.IsSymbolicNotation());
        Assert.True(FilePermissionTypeEnumHelper.IsDefined(permTypeInt));
        Assert.True(FilePermissionTypeEnumHelper.IsDefinedAndValid(permTypeInt));
    }

    [Fact]
    public void ClassificationForInvalidEnum()
    {
        const FilePermissionTypeEnum permType = (FilePermissionTypeEnum)4711;
        const int permTypeInt = (int)permType;
        
        Assert.False(permType.IsDefined());
        Assert.False(permType.IsDefinedAndValid());
        Assert.False(permType.IsOctalNotation());
        Assert.False(permType.IsSymbolicNotation());
        Assert.False(FilePermissionTypeEnumHelper.IsDefined(permTypeInt));
        Assert.False(FilePermissionTypeEnumHelper.IsDefinedAndValid(permTypeInt));
    }
    
    [Fact]
    public void CanCreateUndefinedPermission()
    {
        // _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;

        object perm = FilePermission.UseDefault;

        Assert.IsType<FilePermissionDefault>(perm);
        var permAsFilePermissionValue = perm as FilePermission;
        Assert.NotNull(permAsFilePermissionValue);
        var permAsFilePermissionDefault = perm as FilePermissionDefault;
        Assert.NotNull(permAsFilePermissionDefault);
        var permAsFilePermissionSpecified = perm as FilePermissionSpecified;
        Assert.Null(permAsFilePermissionSpecified);
        var permAsFilePermissionOctal = perm as FilePermissionOctal;
        Assert.Null(permAsFilePermissionOctal);
        var permAsFilePermissionSymbolic = perm as FilePermissionSymbolic;
        Assert.Null(permAsFilePermissionSymbolic);
        var permEnum = permAsFilePermissionValue.FilePermissionType;
        Assert.Equal(FilePermissionTypeEnum.Default, permEnum);
    }
    
    [Fact]
    public void CanCreateOctalPermission()
    {
        // _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;

        object perm = FilePermission.SpecifyOctal("644");

        Assert.IsType<FilePermissionOctal>(perm);
        var permAsFilePermissionValue = perm as FilePermission;
        Assert.NotNull(permAsFilePermissionValue);
        var permAsFilePermissionDefault = perm as FilePermissionDefault;
        Assert.Null(permAsFilePermissionDefault);
        var permAsFilePermissionSpecified = perm as FilePermissionSpecified;
        Assert.NotNull(permAsFilePermissionSpecified);
        var permAsFilePermissionOctal = perm as FilePermissionOctal;
        Assert.NotNull(permAsFilePermissionOctal);
        var permAsFilePermissionSymbolic = perm as FilePermissionSymbolic;
        Assert.Null(permAsFilePermissionSymbolic);
        var permEnum = permAsFilePermissionValue.FilePermissionType;
        Assert.Equal(FilePermissionTypeEnum.Octal, permEnum);
        Assert.Equal("644", permAsFilePermissionSpecified.TextRepresentation);
        Assert.Equal("644", permAsFilePermissionOctal.TextRepresentation);
    }
    
    [Fact]
    public void CanCreateSymbolicPermission()
    {
        // _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;

        object perm = FilePermission.SpecifySymbolic("u+x");

        Assert.IsType<FilePermissionSymbolic>(perm);
        var permAsFilePermissionValue = perm as FilePermission;
        Assert.NotNull(permAsFilePermissionValue);
        var permAsFilePermissionDefault = perm as FilePermissionDefault;
        Assert.Null(permAsFilePermissionDefault);
        var permAsFilePermissionSpecified = perm as FilePermissionSpecified;
        Assert.NotNull(permAsFilePermissionSpecified);
        var permAsFilePermissionOctal = perm as FilePermissionOctal;
        Assert.Null(permAsFilePermissionOctal);
        var permAsFilePermissionSymbolic = perm as FilePermissionSymbolic;
        Assert.NotNull(permAsFilePermissionSymbolic);
        var permEnum = permAsFilePermissionValue.FilePermissionType;
        Assert.Equal(FilePermissionTypeEnum.Symbolic, permEnum);
        Assert.Equal("u+x", permAsFilePermissionSpecified.TextRepresentation);
        Assert.Equal("u+x", permAsFilePermissionSymbolic.TextRepresentation);
    }

    [Fact]
    public void SucceedValidOctal()
    {
        Assert.IsType<FilePermissionOctal>(FilePermission.SpecifyOctal("000"));
        Assert.IsType<FilePermissionOctal>(FilePermission.SpecifyOctal("123"));
        Assert.IsType<FilePermissionOctal>(FilePermission.SpecifyOctal("456"));
        Assert.IsType<FilePermissionOctal>(FilePermission.SpecifyOctal("777"));
    }
    
    [Fact]
    public void FailInvalidOctal()
    {
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifyOctal(""));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifyOctal("6"));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifyOctal("64"));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifyOctal("999"));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifyOctal("7777"));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifyOctal(" 644"));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifyOctal(" 644 "));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifyOctal("644 "));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifyOctal("u+x"));
    }

    [Fact]
    public void SucceedValidSymbolic()
    {
        Assert.IsType<FilePermissionSymbolic>(FilePermission.SpecifySymbolic("u+x"));
        Assert.IsType<FilePermissionSymbolic>(FilePermission.SpecifySymbolic("ugo=rw"));
        Assert.IsType<FilePermissionSymbolic>(FilePermission.SpecifySymbolic("a-w"));
        Assert.IsType<FilePermissionSymbolic>(FilePermission.SpecifySymbolic("ugo=rwx"));
        Assert.IsType<FilePermissionSymbolic>(FilePermission.SpecifySymbolic("ugo=rwx,a-w,go+rx"));
    }

    [Fact]
    public void FailInvalidSymbolic()
    {
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifySymbolic(""));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifySymbolic("a"));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifySymbolic("au+x"));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifySymbolic("uu+x"));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifySymbolic("ux"));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifySymbolic("uuu"));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifySymbolic("g!"));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifySymbolic("777"));
        Assert.Throws<ArgumentException>(() => FilePermission.SpecifySymbolic("2550"));
    }

    #region Helpers

    public FilePermissionTests(ITestOutputHelper testOutputHelper)
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