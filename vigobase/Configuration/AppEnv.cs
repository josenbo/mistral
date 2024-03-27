using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Serilog;

namespace vigobase;

[PublicAPI]
public static class AppEnv
{
    public static FileHandlingParameters DefaultFileHandlingParams { get; }
    public static StandardFileHandling DeployConfigRule { get; private set; }
    public static StandardFileHandling FinalCatchAllRule { get; }
    public static DirectoryInfo TopLevelDirectory
    {
        get => _topLevelDirectory ?? throw new VigoFatalException(AppEnv.Faults.Fatal("FX266","Forgot to set this from the AppConfig in the JobRunner?", string.Empty));
        set => _topLevelDirectory = value;
    }
    public static FileInfo? TimingReportFile { get; set; }
    public static DirectoryInfo TemporaryDirectory { get; }
    public static FileInfo TemporaryDeploymentBundle { get; }
    public static FaultRegistry Faults { get; }

    public static string GetTopLevelRelativePath(string path)
    {
        return Path.GetRelativePath(TopLevelDirectory.FullName, path);
    }
    
    public static string GetTopLevelRelativePath(FileSystemInfo file)
    {
        return Path.GetRelativePath(TopLevelDirectory.FullName, file.FullName);
    }

    public static string GetTemporaryFilePath()
    {
        return Path.Combine(TemporaryDirectory.FullName, $"tempfile_{_tempFileSequence++}");
    }

    public static void CheckAndSetConfigurationFile(string? filenameMarkdown, string? filenameNative)
    {
        if (filenameMarkdown is null && filenameNative is null)
            return;

        var filenames = new List<ConfigurationFilename>();

        if (filenameMarkdown is not null)
        {
            var filenameMarkdownClean = Path.GetFileName(filenameMarkdown);
            if (filenameMarkdownClean != filenameMarkdown)
                throw new VigoFatalException(AppEnv.Faults.Fatal(
                    faultKey: "FX679",
                    supportInfo: $"Path.GetFileName(\"{filenameMarkdown}\") -> \"{filenameMarkdownClean}\"",
                    message: $"The configuration file name for the markdown format '{filenameMarkdown}' is not a valid file name"));

            filenames.Add(new ConfigurationFilename(filenameMarkdown, ConfigurationFileTypeEnum.MarkdownFormat));
        }

        if (filenameNative is not null)
        {
            var filenameNativeClean = Path.GetFileName(filenameNative);
            if (filenameNativeClean != filenameNative)
                throw new VigoFatalException(AppEnv.Faults.Fatal(
                    faultKey: "FX686",
                    supportInfo: $"Path.GetFileName(\"{filenameNative}\") -> \"{filenameNativeClean}\"",
                    message: $"The configuration file name for the native format '{filenameNative}' is not a valid file name"));

            filenames.Add(new ConfigurationFilename(filenameNative, ConfigurationFileTypeEnum.NativeFormat));
        }

        if (0 < filenames.Count)
            DeployConfigRule = DeployConfigRule with { Filenames = filenames };
    }

    public static void RecordTiming(
            string? message = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
    {
        if (TimingReportFile is null)
            return;

        using var sw = TimingReportFile.AppendText();
        sw.WriteLine($"{_currentRunStopwatch.ElapsedMilliseconds}\t{message}\t{memberName}\t{sourceFilePath}\t{sourceLineNumber}");
    }
    
    public static TimeSpan GetCurrentRunElapsedTime()
    {
        return _currentRunStopwatch.Elapsed;
    }
    
    static AppEnv()
    {
        _currentRunStopwatch.Start();

        Faults = new FaultRegistry();
        
        TemporaryDirectory = GetTemporaryDirectory();
        TemporaryDeploymentBundle = new FileInfo(Path.Combine(TemporaryDirectory.FullName, "vigo.tar.gz"));
        
        var asciiGerman = ValidCharactersHelper.ParseConfiguration("AsciiGerman");
    
        DefaultFileHandlingParams = new FileHandlingParameters(
            StandardModeForFiles: (UnixFileMode)0b_110_110_100,
            StandardModeForDirectories: (UnixFileMode)0b_111_111_101,
            FileType: FileTypeEnum.BinaryFile, 
            SourceFileEncoding: FileEncodingEnum.UTF_8,
            TargetFileEncoding: FileEncodingEnum.UTF_8,
            LineEnding: LineEndingEnum.LF,
            Permissions: FilePermission.UseDefault, 
            FixTrailingNewline: true,
            ValidCharsRegex: asciiGerman,
            Targets: [ "Prod", "NonProd" ]
        );

        FinalCatchAllRule = new StandardFileHandling(
            Filenames: Array.Empty<ConfigurationFilename>(),
            Handling: DefaultFileHandlingParams with 
            {
                FileType = FileTypeEnum.BinaryFile,
                Permissions = FilePermission.UseDefault
            },
            DoCopy:false);

        DeployConfigRule = new StandardFileHandling(
            Filenames: new List<ConfigurationFilename>()
            {
                new ConfigurationFilename("deployment-rules.md", ConfigurationFileTypeEnum.MarkdownFormat),
                new ConfigurationFilename("deployment-rules.vigo", ConfigurationFileTypeEnum.NativeFormat)
            },
            Handling: DefaultFileHandlingParams with
            {
                FileType = FileTypeEnum.BinaryFile,
                Permissions = FilePermission.UseDefault
            }, 
            DoCopy: false);
    }

    public static DirectoryInfo GetTemporaryDirectory()
    {
        var envValue = Environment.GetEnvironmentVariable(EnvVarVigoTempDir);
        Log.Debug("{TheProperty} = {TheValue}", nameof(envValue), envValue);
        var path = envValue ?? Path.GetTempPath();
        Log.Debug("{TheProperty} = {TheValue}", nameof(path), path);

        if (4096 < path.Length)
            path = path[..4096];

        if (!Directory.Exists(path))
            throw new VigoFatalException(AppEnv.Faults.Fatal("FX273","Check if the path came from system or settings and see how to handle this case properly", string.Empty));
            
        if (!Path.IsPathRooted(path))
            path = Path.GetFullPath(path);
        
        var formatProvider = CultureInfo.GetCultureInfo("en-US");
        // ReSharper disable StringLiteralTypo
        var timestamp = DateTime.Now.ToString("DyyyyMMdd_THHmmss", formatProvider);
        // ReSharper restore StringLiteralTypo
        var random = Random.Shared.Next(1,99999).ToString("00000", formatProvider);
        var name = $"VIGO_{timestamp}_R{random}";

        path = Path.Combine(path, name);
        
        var directoryInfo = new DirectoryInfo(path);

        if (directoryInfo.Exists)
            throw new VigoFatalException(AppEnv.Faults.Fatal(
                "FX280",
                "Collisions are possible but improbable. Check what happened and consider adding collision detection or better cleanup", 
                string.Empty));
        
        directoryInfo.Create();
        
        return directoryInfo;
    }

    public static bool IsApplicationModulePath(string path)
    {
        if (ProcessDirectoryPath is null)
            return false;

        if (!path.StartsWith(ProcessDirectoryPath))
            return false;
        
        var matchedModule = Process
            .GetCurrentProcess()
            .Modules
            .OfType<ProcessModule>()
            .SingleOrDefault(m => m.FileName == path);

        return (matchedModule is not null);
    }
    
    private const string EnvVarVigoTempDir = "VIGO_TEMP";
    
    private static DirectoryInfo? _topLevelDirectory;
    private static int _tempFileSequence = Random.Shared.Next(100000000, 999999999);
    private static readonly string? ProcessDirectoryPath =
        Environment.ProcessPath is not null && File.Exists(Environment.ProcessPath)
            ? new FileInfo(Environment.ProcessPath).DirectoryName
            : null;

    private static Stopwatch _currentRunStopwatch = new Stopwatch();
}