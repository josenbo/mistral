namespace vigo;

internal class PartialProgramArguments
{
    public string? RepositoryRootPath { get; set; }
    public string? DeploymentBundlePath { get; set; }
    public string? TargetNames { get; set; }
    public bool? IncludePrepared { get; set; }
    public string? MappingReportFilePath { get; set; }
    public string? ExplainName { get; set; }
    public bool? ShowHelp { get; set; }
    public bool? ShowVersion { get; set; }
}