using Serilog;
using Serilog.Events;

namespace vigo.Startup;

internal static class ProgramArgumentHandler
{
    public static AppConfig GetProgramTaskFromCommandLineAndEnvironmentVariables() 
        => GetProgramTask(
            commandLineArguments: Environment.GetCommandLineArgs(), 
            environmentVariables: EnvVar.GetSystem()
            );

    public static AppConfig GetProgramTaskFromCommandLineAndEnvironmentVariables(
        IEnumerable<string> commandLineArguments, 
        EnvVar environmentVariables
        ) 
        => GetProgramTask(
            commandLineArguments, 
            environmentVariables
            );
    private static AppConfig GetProgramTask(IEnumerable<string> commandLineArguments, EnvVar environmentVariables)
    {
        var cmdline = commandLineArguments.ToArray();

        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Debug("Entering {TheAssembly}.{TheClass}.{TheMethod}",
                typeof(ProgramArgumentHandler).Assembly.FullName,
                nameof(ProgramArgumentHandler),
                nameof(GetProgramTaskFromCommandLineAndEnvironmentVariables));
            
            Log.Debug("Command line arguments:");
            
            for (var i = 0; i < cmdline.Length; i++)
            {
                Log.Debug("Argument {TheIndex}: {TheValue}", 
                    i,
                    cmdline[i]
                    );    
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

        return new AppConfigInfoVersion();
    }

    private static void Get
    private static bool FilterEnvironmentVariablesForLogging(string environmentVariable)
    {
        return environmentVariable.StartsWith("VIGO_");
    }
}

internal class CombinedAndSanitizedProgramArguments
{
    public string? RepositoryRootPath { get; set; }
    public string? DeploymentBundlePath { get; set; }
    public string? TargetNames { get; set; }
    public bool MinimalWork { get; set; }
    
}