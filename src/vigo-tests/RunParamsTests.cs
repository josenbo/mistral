using System.Text;
using vigo;
// ReSharper disable StringLiteralTypo

namespace vigo_tests;

public class RunParamsTests : IDisposable
{
    // Temporary file and folder layout, which is set up before running the tests
    //
    // .                                    (_temporaryLocalDirectoryToBeDeletedAfterTheTestRun)
    // +-- Uno                              (Current working directory)
    // +-- Due
    //     +-- Repo                         (_temporaryRepositoryRootPath)
    //     |   +-- deployment-rules.vigo    (_temporaryConfigurationFilePath)
    //     +-- Out                          
    //         +-- sample.tar.gz            (_temporaryOutputFilePath)
    // 
    // Setup in constructor
    // Cleanup in Dispose()
    
    [Fact]
    public void Test1()
    {
        _logLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        string[] cmdArgs = [
            "deploy", 
            "-R", 
            Path.Combine("..", "Due", "Repo"), 
            "-O", 
            Path.Combine("..", "Due", "Out", "sample.tar.gz")
        ];
        var envMock = EnvVar.GetMock()
            .Add("VIGO_TARGETS", "eins");
        
        var appConfig = AppConfigBuilder.Build(cmdArgs, envMock);

        Assert.NotNull(appConfig);
    }
    
    #region Helpers

    public RunParamsTests(ITestOutputHelper testOutputHelper)
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
        
        var uniqueName = $"vigo-test-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100000000, 999999999)}";
        var tmpRootPath = Path.Combine(Path.GetTempPath(), uniqueName);
        Directory.CreateDirectory(tmpRootPath);
        _temporaryLocalDirectoryToBeDeletedAfterTheTestRun = new DirectoryInfo(tmpRootPath); 
        
        var tmpUnoPath = Path.Combine(tmpRootPath, "Uno");
        Directory.CreateDirectory(tmpUnoPath);
        _previousCurrentDirectory = Environment.CurrentDirectory; 
        Environment.CurrentDirectory = tmpUnoPath;
        
        var tmpDuePath = Path.Combine(tmpRootPath, "Due");
        Directory.CreateDirectory(tmpDuePath);
        
        _temporaryRepositoryRootPath = Path.Combine(tmpDuePath, "Repo");
        Directory.CreateDirectory(_temporaryRepositoryRootPath);
        
        _temporaryConfigurationFilePath = Path.Combine(_temporaryRepositoryRootPath, "deployment-rules.vigo");
        File.WriteAllText(_temporaryConfigurationFilePath, "", Encoding.UTF8);
        
        var tmpDueOutPath = Path.Combine(tmpDuePath, "Out");
        Directory.CreateDirectory(tmpDueOutPath);
        
        _temporaryOutputFilePath = Path.Combine(tmpDueOutPath, "sample.tar.gz");
        File.WriteAllBytes(_temporaryOutputFilePath, Array.Empty<byte>());
    }

    public void Dispose()
    {
        Environment.CurrentDirectory = _previousCurrentDirectory;
        _temporaryLocalDirectoryToBeDeletedAfterTheTestRun.Refresh();
        if (_temporaryLocalDirectoryToBeDeletedAfterTheTestRun.Exists)
            _temporaryLocalDirectoryToBeDeletedAfterTheTestRun.Delete(recursive: true);
    }
    
    // ReSharper disable once NotAccessedField.Local
    private readonly ITestOutputHelper _testOutputHelper;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly LoggingLevelSwitch _logLevelSwitch;

    private readonly DirectoryInfo _temporaryLocalDirectoryToBeDeletedAfterTheTestRun;
    private readonly string _temporaryRepositoryRootPath;
    private readonly string _temporaryOutputFilePath;
    private readonly string _temporaryConfigurationFilePath;
    private readonly string _previousCurrentDirectory;

    #endregion
}