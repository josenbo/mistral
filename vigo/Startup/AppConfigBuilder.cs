using System.Text;
using CommandLine;
using Serilog;
using Serilog.Events;
using vigobase;

namespace vigo;

internal static class AppConfigBuilder
{
    public static AppConfig Assemble(IEnumerable<string> cmdArgsEnum) 
        => BuildAppConfig(cmdArgsEnum, envVarFacade: EnvVar.GetSystem());

    public static AppConfig Assemble(IEnumerable<string> cmdArgsEnum, EnvVar envVarFacade) 
        => BuildAppConfig(cmdArgsEnum, envVarFacade);
    
    private static AppConfig BuildAppConfig(IEnumerable<string> cmdArgsEnum, EnvVar envVarFacade)
    {
        var cmdArgs = cmdArgsEnum.ToArray();

        LogCommandLineArgumentsAndEnvironmentVariables(cmdArgs, envVarFacade);
        var partialArguments = ParsePartialProgramTaskArguments(cmdArgs);
        EnrichMissingArgumentsFromEnvironmentVariables(envVarFacade, partialArguments);
        
        var retval =  CreateAppConfig(partialArguments);
        retval.LogObject();
        return retval;
    }

    private static PartialProgramArguments ParsePartialProgramTaskArguments(string[] commandLineArguments)
    {
        // Parse the command line arguments and populate
        // the PartialProgramTaskArguments object with
        // the parsed values

        var sb = new StringBuilder();
        var sw = new StringWriter(sb);
        
        var parser = new Parser(config =>
        {
            config.AutoHelp = false;
            config.AutoVersion = false;
            config.HelpWriter = sw;
        });

        var parserResult = parser.ParseArguments<PartialProgramArguments>(commandLineArguments);

        if (parserResult?.Value is null)
            throw new VigoFatalException(AppEnv.Faults.Fatal(
                "FX651",
                sb.ToString(),
                "Encountered invalid command line arguments"));

        var retval = parserResult.Value ?? throw new VigoFatalException(AppEnv.Faults.Fatal("FX658", "This condition was checked, handled and should never occur", string.Empty));
        
        Log.Debug("Parsed command line:");
        Log.Debug("{TheParam} = {TheValue}", nameof(retval.RepositoryRootPath), retval.RepositoryRootPath);
        Log.Debug("{TheParam} = {TheValue}", nameof(retval.DeploymentBundlePath), retval.DeploymentBundlePath);
        Log.Debug("{TheParam} = {TheValue}", nameof(retval.TargetNames), retval.TargetNames);
        Log.Debug("{TheParam} = {TheValue}", nameof(retval.Preview), retval.Preview);
        Log.Debug("{TheParam} = {TheValue}", nameof(retval.MappingReportFilePath), retval.MappingReportFilePath);
        // Log.Debug("{TheParam} = {TheValue}", nameof(retval.ExplainName), retval.ExplainName);
        Log.Debug("{TheParam} = {TheValue}", nameof(retval.ShowHelp), retval.ShowHelp);
        Log.Debug("{TheParam} = {TheValue}", nameof(retval.ShowVersion), retval.ShowVersion);

        return retval;
    }

    private static void EnrichMissingArgumentsFromEnvironmentVariables(EnvVar envVarFacade, PartialProgramArguments partialArguments)
    {
        if (string.IsNullOrWhiteSpace(partialArguments.RepositoryRootPath) && envVarFacade.TryGetEnvironmentVariable("VIGO_REPOSITORY", out var parsedRepositoryRootPath))
        {
            partialArguments.RepositoryRootPath = LimitStringLength(4096, parsedRepositoryRootPath);

            Log.Debug("Copy environment variable {TheEnvVar} into {TheParam} = {TheValue}", 
                "VIGO_REPOSITORY",
                nameof(partialArguments.RepositoryRootPath), 
                partialArguments.RepositoryRootPath);
        }
        
        if (string.IsNullOrWhiteSpace(partialArguments.DeploymentBundlePath) && envVarFacade.TryGetEnvironmentVariable("VIGO_BUNDLE", out var parsedDeploymentBundlePath))
        {
            partialArguments.DeploymentBundlePath = LimitStringLength(4096, parsedDeploymentBundlePath);

            Log.Debug("Copy environment variable {TheEnvVar} into {TheParam} = {TheValue}", 
                "VIGO_BUNDLE",
                nameof(partialArguments.DeploymentBundlePath), 
                partialArguments.DeploymentBundlePath);
        }
        
        if (string.IsNullOrWhiteSpace(partialArguments.TargetNames) && envVarFacade.TryGetEnvironmentVariable("VIGO_TARGETS", out var parsedTargetNames))
        {
            partialArguments.TargetNames = LimitStringLength(4096, parsedTargetNames);

            Log.Debug("Copy environment variable {TheEnvVar} into {TheParam} = {TheValue}", 
                "VIGO_TARGETS",
                nameof(partialArguments.TargetNames), 
                partialArguments.TargetNames);
        }

        if (!partialArguments.Preview && envVarFacade.TryGetEnvironmentVariable("VIGO_PREVIEW", out _))
        {
            partialArguments.Preview = true;

            Log.Debug("Copy environment variable {TheEnvVar} into {TheParam} = {TheValue}", 
                "VIGO_PREVIEW",
                nameof(partialArguments.Preview), 
                partialArguments.Preview);
        }

        if (string.IsNullOrWhiteSpace(partialArguments.MappingReportFilePath) && envVarFacade.TryGetEnvironmentVariable("VIGO_REPORT", out var parsedMappingReportFilePath))
        {
            partialArguments.MappingReportFilePath = LimitStringLength(4096, parsedMappingReportFilePath);

            Log.Debug("Copy environment variable {TheEnvVar} into {TheParam} = {TheValue}", 
                "VIGO_REPORT",
                nameof(partialArguments.MappingReportFilePath), 
                partialArguments.MappingReportFilePath);
        }
    }

    private static string? LimitStringLength(int maxLength, string? value)
    {
        if (value is null || value.Length < maxLength)
            return value;
        
        return value[..maxLength];
    }
    
    private static AppConfig CreateAppConfig(PartialProgramArguments partialArguments)
    {
        // Create an instance of AppConfigInfo using the
        // values from the PartialProgramTaskArguments object

        if (partialArguments.ShowHelp)
            return new AppConfigHelp();

        if (partialArguments.ShowVersion)
            return new AppConfigVersion();

        if (string.IsNullOrWhiteSpace(partialArguments.RepositoryRootPath) || !Directory.Exists(partialArguments.RepositoryRootPath))
            throw new VigoFatalException(AppEnv.Faults.Fatal("FX630", null, "The repository root argument is missing or does not point to an existing directory"));

        var repositoryRoot = new DirectoryInfo(Path.GetFullPath(partialArguments.RepositoryRootPath));

        // if (!string.IsNullOrWhiteSpace(partialArguments.ExplainName))
        //     return new AppConfigExplain(repositoryRoot, partialArguments.ExplainName);

        FileInfo? deploymentBundle = null;

        if (!string.IsNullOrWhiteSpace(partialArguments.DeploymentBundlePath))
        {
            var filePath = Path.GetFullPath(partialArguments.DeploymentBundlePath);
            var folderPath = Path.GetDirectoryName(filePath);
            
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                throw new VigoFatalException(AppEnv.Faults.Fatal("FX637", null, "The deployment bundle argument must be a file path in an existing directory"));
            
            deploymentBundle = new FileInfo(filePath);
        }

        var targets = DeploymentTargetHelper.ParseTargets(partialArguments.TargetNames).ToList();

        var preview = partialArguments.Preview;
        
        FileInfo? mappingReport = null;
        
        if (!string.IsNullOrWhiteSpace(partialArguments.MappingReportFilePath))
        {
            var filePath = Path.GetFullPath(partialArguments.MappingReportFilePath);
            var folderPath = Path.GetDirectoryName(filePath);
            
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                throw new VigoFatalException(AppEnv.Faults.Fatal("FX644", null, "The mapping report argument must be a file path in an existing directory"));
            
            mappingReport = new FileInfo(filePath);
        }

        return new AppConfigDeploy(repositoryRoot, deploymentBundle, targets, preview, mappingReport);
    }

    private static void LogCommandLineArgumentsAndEnvironmentVariables(
        IEnumerable<string> commandLineArguments,
        EnvVar environmentVariables)
    {
        if (!Log.IsEnabled(LogEventLevel.Debug))
            return;
        
        Log.Debug("Entering {TheAssembly}.{TheClass}.{TheMethod}",
            typeof(AppConfigBuilder).Assembly.FullName,
            nameof(AppConfigBuilder),
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