namespace vigocfg;

public interface IAppUserFileStore
{
    string GroupName { get; }
    DirectoryInfo SourceFolder { get; }
    DirectoryInfo? TargetFolder { get; }
    bool CreateTargetFolder { get; }
    bool CleanTargetFolder { get; }
    IList<ITransferRule> Rules { get; }
}