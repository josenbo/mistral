using Serilog;
using vigobase;

namespace vigo;

internal static class AppConfigBuilder
{
    public static AppConfig Build() 
        => Assemble(Environment.GetCommandLineArgs(), EnvVar.GetSystem());
    
    public static AppConfig Build(IEnumerable<string> cmdArgs) 
        => Assemble(cmdArgs, EnvVar.GetSystem());
    
    public static AppConfig Build(EnvVarMock mock) 
        => Assemble(Environment.GetCommandLineArgs(), mock);
    
    public static AppConfig Build(IEnumerable<string> cmdArgs, EnvVarMock mock) 
        => Assemble(cmdArgs, mock);

    private static AppConfig Assemble(IEnumerable<string> cmdArgs, EnvVar envVarSource)
    {
        var readerCmdLine = new ConfigSourceReaderCommandLine(cmdArgs);
        
        var readerEnvVar = envVarSource switch
        {
            EnvVarMock mock => new ConfigSourceReaderEnvironmentVariables(mock),
            _ => new ConfigSourceReaderEnvironmentVariables()
        };

        var appArgs = readerCmdLine.Read(AppArguments.Empty);
        Log.Debug("Args from command line {TheAppArgs}", appArgs);
        
        appArgs = readerEnvVar.Read(appArgs);
        Log.Debug("Args from command line and environment variables {TheAppArgs}", appArgs);
        
        return appArgs.Command switch
        {
            CommandEnum.Deploy => AssembleDeploy(appArgs),
            CommandEnum.Check => AssembleCheck(appArgs),
            CommandEnum.Explain => AssembleExplain(appArgs),
            CommandEnum.Help => AssembleHelp(appArgs),
            CommandEnum.Version => AssembleVersion(appArgs),
            CommandEnum.Undefined => AssembleFault(appArgs),
            null => AssembleFault(appArgs),
            _ => AssembleFault(appArgs)
        };
    }

    private static AppConfigRepoDeploy AssembleDeploy(AppArguments appArgs)
    {
        var repositoryRoot = appArgs.RepositoryRoot
                             ?? throw new VigoFatalException(AppEnv.Faults.Fatal("The repository root is mandatory for the deploy action"));
        
        var deploymentBundle = appArgs.OutputFile
                               ?? throw new VigoFatalException(AppEnv.Faults.Fatal("The output file is mandatory for the deploy action"));
        
        var targets = appArgs.Targets ?? Array.Empty<string>();
                      
        var appConfig = new AppConfigRepoDeploy(
            RepositoryRoot: repositoryRoot,
            OutputFile: deploymentBundle,
            Targets: targets);
        
        Log.Debug("AppConfig was read as {TheAppConfig}", appConfig);
        return appConfig;
    }

    private static AppConfigRepoCheck AssembleCheck(AppArguments appArgs)
    {
        var repositoryRoot = appArgs.RepositoryRoot
                             ?? throw new VigoFatalException(AppEnv.Faults.Fatal("The repository root is mandatory for the check action"));

        var appConfig = new AppConfigRepoCheck(
            RepositoryRoot: repositoryRoot);
        
        Log.Debug("AppConfig was read as {TheAppConfig}", appConfig);
        return appConfig;
    }

    private static AppConfigFolderExplain AssembleExplain(AppArguments appArgs)
    {
        var configurationFile = appArgs.ConfigurationFile
                                ?? throw new VigoFatalException(AppEnv.Faults.Fatal("The configuration file is mandatory for the explain action"));
        
        var names = appArgs.Names ?? Array.Empty<string>();
        
        var appConfig = new AppConfigFolderExplain(
            ConfigurationFile: configurationFile,
            Names: names);
        
        Log.Debug("AppConfig was read as {TheAppConfig}", appConfig);
        return appConfig;
    }

    private static AppConfigInfoHelp AssembleHelp(AppArguments appArgs)
    {
        var commandToShowHelpFor = appArgs.CommandToShowHelpFor ?? CommandEnum.Undefined;
        
        var appConfig = new AppConfigInfoHelp(
            CommandToShowHelpFor: commandToShowHelpFor);
        
        Log.Debug("AppConfig was read as {TheAppConfig}", appConfig);
        return appConfig;
    }

    private static AppConfigInfoVersion AssembleVersion(AppArguments appArgs)
    {
        var appConfig = new AppConfigInfoVersion();
        
        Log.Debug("AppConfig was read as {TheAppConfig}", appConfig);
        return appConfig;
    }

    private static AppConfig AssembleFault(AppArguments appArgs)
    {
        Log.Error("Invalid or missing action {TheAppArgs}", appArgs);
        throw new VigoFatalException(AppEnv.Faults.Fatal("Invalid or missing action"));
    }
}