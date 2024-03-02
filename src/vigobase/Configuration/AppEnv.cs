using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;
using Serilog;

namespace vigobase;

[PublicAPI]
public static class AppEnv
{
    public static FileHandlingParameters DefaultFileHandlingParams { get; set; }
    public static StandardFileHandling DeployConfigRule { get; set; }
    public static StandardFileHandling FinalCatchAllRule { get; set; }
    public static DirectoryInfo TopLevelDirectory
    {
        get => _topLevelDirectory ?? throw new VigoFatalException(AppEnv.Faults.Fatal("FX266","Forgot to set this from the AppConfig in the JobRunner?", string.Empty));
        set => _topLevelDirectory = value;
    }
    public static DirectoryInfo TemporaryDirectory { get; set; }
    public static FaultRegistry Faults { get; set; }

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

    static AppEnv()
    {
        Faults = new FaultRegistry();
        
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

        TemporaryDirectory = GetTemporaryDirectory();
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
}