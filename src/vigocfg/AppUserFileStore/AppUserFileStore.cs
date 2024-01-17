using Ardalis.GuardClauses;

namespace vigocfg;

internal class AppUserFileStore : IAppUserFileStore
{
    public string GroupName { get; }
    public DirectoryInfo SourceFolder { get; }
    public DirectoryInfo? TargetFolder { get; }
    public bool CreateTargetFolder { get; }
    public bool CleanTargetFolder { get; }
    public IList<ITransferRule> Rules { get; }

    internal AppUserFileStore(DirectoryInfo source, DirectoryInfo? target, string groupName, bool createTarget, bool cleanTarget, IEnumerable<TransferRule> rules)
    {
        GroupName = Guard.Against.NullOrWhiteSpace(groupName);
        SourceFolder = Guard.Against.InvalidInput(source, nameof(source), d => d.Exists);
        if (target is not null)
            TargetFolder = Guard.Against.InvalidInput(target, nameof(target), d => createTarget ? d.Parent is { Exists: true } : d.Exists);
        CreateTargetFolder = createTarget;
        CleanTargetFolder = cleanTarget;
        Rules = new List<ITransferRule>(rules);
    }
}