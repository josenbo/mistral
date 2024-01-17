using Ardalis.GuardClauses;

namespace vigocfg;

internal class AppUser : IAppUser
{
    public string UserName { get; }
    public DirectoryInfo SourceFolder { get; }
    public DirectoryInfo? TargetFolder { get; }
    public bool CleanTargetFolder { get; }
    public IEnumerable<string> ActiveTags => _activeTags.Values;
    public IEnumerable<IAppUserFileStore> FileStores => _fileStores;

    public AppUser(string username, DirectoryInfo source, DirectoryInfo? target, bool cleanTarget)
    {
        UserName = Guard.Against.InvalidInput(username, nameof(username), n => n is "cupines" or "stammaus");
        SourceFolder = Guard.Against.InvalidInput(source, nameof(source), d => d.Exists);
        TargetFolder = target is null
            ? null
            : Guard.Against.InvalidInput(target, nameof(target), d => d.Exists);
        CleanTargetFolder = cleanTarget;
        _activeTags.Add(UserName, UserName);
        foreach (var tag in EnvironmentHelper.StagingTags)
        {
            _activeTags.TryAdd(tag, tag);
        }

        foreach (var foldername in new[]{ "crontab", "local-bin", "local-data" })
        {
            var fileStoreSource = new DirectoryInfo(Path.Combine(SourceFolder.FullName, foldername));
            if (!fileStoreSource.Exists)
                continue;
            var fileStoreTarget = TargetFolder is null 
                ? null 
                : new DirectoryInfo(Path.Combine(TargetFolder.FullName, foldername));
            var rules = TransferRulesFactory.GetRules(UserName, foldername);
            _fileStores.Add(new AppUserFileStore(fileStoreSource, fileStoreTarget, foldername, true, true, rules));
        }
    }

    private readonly List<AppUserFileStore> _fileStores = new();
    private readonly Dictionary<string, string> _activeTags = new(StringComparer.InvariantCultureIgnoreCase);
}