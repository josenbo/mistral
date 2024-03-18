using CommandLine;
namespace vigo;

internal class PartialProgramArguments
{
    [Value(0)]
    public string? RepositoryRootPath { get; set; }
    
    [Option('b', "bundle")]
    public string? DeploymentBundlePath { get; set; }
    
    [Option('t', "targets")]
    public string? TargetNames { get; set; }
    
    [Option('p', "preview", Default = (bool)false)]
    public bool Preview { get; set; }
    
    [Option('r', "report")]
    public string? MappingReportFilePath { get; set; }
    
    [Option('n', "name")]
    public string? ExplainName { get; set; }
    
    [Option('h', "help", Default = (bool)false)]
    public bool ShowHelp { get; set; }
    
    [Option('v', "version", Default = (bool)false)]
    public bool ShowVersion { get; set; }
}