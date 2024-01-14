namespace vigocfg;

public interface IAppUser
{
    string UserName { get; }
    DirectoryInfo SourceFolder { get; }
    DirectoryInfo? TargetFolder { get; }
    bool CleanTargetFolder { get; }
    IEnumerable<string> ActiveTags { get; }
    IEnumerable<IAppUserFileStore> FileStores { get; }
}