using Serilog;
using Serilog.Events;
using vigobase;

namespace vigo;

internal static class ProgramArguments
{
    public static AppConfig Assemble() 
        => BuildAppConfig(cmdArgsEnum: Environment.GetCommandLineArgs(), envVarFacade: EnvVar.GetSystem());

    public static AppConfig Assemble(IEnumerable<string> cmdArgsEnum, EnvVar envVarFacade) 
        => BuildAppConfig(cmdArgsEnum, envVarFacade);
    
    private static AppConfig BuildAppConfig(IEnumerable<string> cmdArgsEnum, EnvVar envVarFacade)
    {
        var cmdArgs = cmdArgsEnum.ToArray();

        LogCommandLineArgumentsAndEnvironmentVariables(cmdArgs, envVarFacade);
        var partialArguments = ParsePartialProgramTaskArguments(cmdArgs);
        EnrichMissingArgumentsFromEnvironmentVariables(envVarFacade, partialArguments);
        var retval =  CreateAppConfigInfo(partialArguments);
        retval.LogObject();
        return retval;
    }

    private static PartialProgramArguments ParsePartialProgramTaskArguments(string[] commandLineArguments)
    {
        // Parse the command line arguments and populate
        // the PartialProgramTaskArguments object with
        // the parsed values
        
        return new PartialProgramArguments();
    }

    private static void EnrichMissingArgumentsFromEnvironmentVariables(EnvVar envVarFacade, PartialProgramArguments partialArguments)
    {
        if (string.IsNullOrWhiteSpace(partialArguments.RepositoryRootPath) && envVarFacade.TryGetEnvironmentVariable("VIGO_REPOSITORY_ROOT", out var parsedRepositoryRootPath))
            partialArguments.RepositoryRootPath = LimitStringLength(4096, parsedRepositoryRootPath);
        
        if (string.IsNullOrWhiteSpace(partialArguments.DeploymentBundlePath) && envVarFacade.TryGetEnvironmentVariable("VIGO_DEPLOYMENT_BUNDLE", out var parsedDeploymentBundlePath))
            partialArguments.DeploymentBundlePath = LimitStringLength(4096, parsedDeploymentBundlePath);
        
        if (string.IsNullOrWhiteSpace(partialArguments.TargetNames) && envVarFacade.TryGetEnvironmentVariable("VIGO_TARGETS", out var parsedTargetNames))
            partialArguments.TargetNames = LimitStringLength(4096, parsedTargetNames);

        if (partialArguments.IncludePrepared is null && envVarFacade.TryGetEnvironmentVariable("VIGO_INCLUDE_PREPARED", out var parsedIncludePrepared))
            partialArguments.IncludePrepared = true;

        if (string.IsNullOrWhiteSpace(partialArguments.MappingReportFilePath) && envVarFacade.TryGetEnvironmentVariable("VIGO_MAPPING_REPORT", out var parsedMappingReportFilePath))
            partialArguments.MappingReportFilePath = LimitStringLength(4096, parsedMappingReportFilePath);
    }

    private static string? LimitStringLength(int maxLength, string? value)
    {
        if (value is null || value.Length < maxLength)
            return value;
        
        return value[..maxLength];
    }
    
    private static AppConfig CreateAppConfigInfo(PartialProgramArguments partialArguments)
    {
        // Create an instance of AppConfigInfo using the
        // values from the PartialProgramTaskArguments object

        if (partialArguments.ShowHelp is true)
            return new AppConfigHelp();

        if (partialArguments.ShowVersion is true)
            return new AppConfigVersion();

        if (string.IsNullOrWhiteSpace(partialArguments.RepositoryRootPath) || !Directory.Exists(partialArguments.RepositoryRootPath))
            throw new VigoFatalException(AppEnv.Faults.Fatal("FX630", null, "The repository root argument is missing or does not point to an existing directory"));

        var repositoryRoot = new DirectoryInfo(Path.GetFullPath(partialArguments.RepositoryRootPath));

        if (!string.IsNullOrWhiteSpace(partialArguments.ExplainName))
            return new AppConfigExplain(repositoryRoot, partialArguments.ExplainName);

        FileInfo? deploymentBundle = null;

        if (!string.IsNullOrWhiteSpace(partialArguments.DeploymentBundlePath))
        {
            var filepath = Path.GetFullPath(partialArguments.DeploymentBundlePath);
            var folderpath = Path.GetDirectoryName(filepath);
            
            if (string.IsNullOrWhiteSpace(folderpath) || !Directory.Exists(folderpath))
                throw new VigoFatalException(AppEnv.Faults.Fatal("FX637", null, "The deployment bundle argument must be a file path in an existing directory"));
            
            deploymentBundle = new FileInfo(filepath);
        }

        var targets = DeploymentTargetHelper.ParseTargets(partialArguments.TargetNames).ToList();

        var includePrepared = (partialArguments.IncludePrepared is true);
        
        FileInfo? mappingReport = null;
        
        if (!string.IsNullOrWhiteSpace(partialArguments.MappingReportFilePath))
        {
            var filepath = Path.GetFullPath(partialArguments.MappingReportFilePath);
            var folderpath = Path.GetDirectoryName(filepath);
            
            if (string.IsNullOrWhiteSpace(folderpath) || !Directory.Exists(folderpath))
                throw new VigoFatalException(AppEnv.Faults.Fatal("FX644", null, "The mapping report argument must be a file path in an existing directory"));
            
            mappingReport = new FileInfo(filepath);
        }

        return new AppConfigDeploy(repositoryRoot, deploymentBundle, targets, includePrepared, mappingReport);
    }

    private static void LogCommandLineArgumentsAndEnvironmentVariables(
        IEnumerable<string> commandLineArguments,
        EnvVar environmentVariables)
    {
        if (!Log.IsEnabled(LogEventLevel.Debug))
            return;
        
        Log.Debug("Entering {TheAssembly}.{TheClass}.{TheMethod}",
            typeof(ProgramArguments).Assembly.FullName,
            nameof(ProgramArguments),
            nameof(Assemble));
        
        Log.Debug("Command line arguments:");

        var i = 0;
        foreach (var arg in commandLineArguments)
        {
            Log.Debug("Argument {TheIndex}: {TheValue}", i, arg);
            i++;
        }
        
        Log.Debug("Environment variables:");

        foreach (var environmentVariable in environmentVariables.GetEnvironmentVariables()
                     .Where(FilterEnvironmentVariablesForLogging)
                     .Order())
        {
            Log.Debug("{TheName}: {TheValue}", 
                environmentVariable,
                environmentVariables.GetEnvironmentVariable(environmentVariable)
            );    
        }
    }
    
    private static bool FilterEnvironmentVariablesForLogging(string environmentVariable)
    {
        return environmentVariable.StartsWith("VIGO_");
    }
}