using Ardalis.GuardClauses;
using Serilog;
using vigocfg;
using vigoftg;

namespace vigolib;

internal class AppUserWork: IWork
{
    public string Name { get; }
    internal AppUserWork(IAppUser appUser, IVigoConfig config, INameParser nameParser)
    {
        _appUser = appUser;
        _nameParser = nameParser;
        _config = config;
        Name = $"Prepare Deployment for {appUser.UserName}";
    }

    public bool Prepare()
    {
        Log.Information("Preparing the deployment of {UserName} cronjobs and scripts", _appUser.UserName);
        Log.Debug("Entering the phase {PhaseName} of the task {TaskName}", nameof(Prepare), Name);

        if (_appUser.TargetFolder is null)
            throw new NullReferenceException($"{nameof(_appUser.TargetFolder)} should not be null at this point. Initial checks missing?");
        
        if (!_appUser.TargetFolder.Exists)
        {
            Log.Error("The staging folder for {UserName} deployments is required to exist. Path: {DistroPath}",
                _appUser.UserName,
                _appUser.TargetFolder.FullName);
            
            return false;
        }
        
        if (_appUser.CleanTargetFolder)
            if (!_appUser.TargetFolder.PurgeContents())
            {
                Log.Error("Could not purge the contents in {UserName} staging folder. Path: {DistroPath}",
                    _appUser.UserName,
                    _appUser.TargetFolder.FullName);
            
                return false;
            }
            else Log.Debug("Purged the contents in {UserName} staging folder",
                _appUser.UserName);

        Log.Debug("Leaving the phase {PhaseName} of the task {TaskName}", nameof(Prepare), Name);
        return true;
    }

    public bool Execute()
    {
        Log.Debug("Entering the phase {PhaseName} of the task {TaskName}", nameof(Execute), Name);

        foreach (var fileStore in _appUser.FileStores)
        {
            fileStore.SourceFolder.Refresh();
            
            if (!fileStore.SourceFolder.Exists)
            {
                Log.Error("Missing source folder {FolderName} for {UserName}. Path: {SourcePath}",
                    fileStore.SourceFolder.Name,
                    _appUser.UserName,
                    fileStore.SourceFolder.FullName);
            
                return false;
            }
            
            if (fileStore.TargetFolder is null)
                throw new NullReferenceException($"{nameof(fileStore.TargetFolder)} should not be null at this point. Initial checks missing?");
        
            fileStore.TargetFolder.Refresh();

            if (!fileStore.TargetFolder.Exists)
            {
                if (!fileStore.CreateTargetFolder)
                {
                    Log.Error("Target folder {FolderName} for {UserName} is required to exist. Path: {TargetPath}",
                        fileStore.TargetFolder.Name,
                        _appUser.UserName,
                        fileStore.TargetFolder.FullName);
            
                    return false;
                }

                if (!fileStore.TargetFolder.CreateFolderIfMissing())
                {
                    Log.Error("Target folder {FolderName} for {UserName} could not be created. Path: {TargetPath}",
                        fileStore.TargetFolder.Name,
                        _appUser.UserName,
                        fileStore.TargetFolder.FullName);
            
                    return false;
                }
                else Log.Debug("Created target folder {FolderName} for {UserName}",
                    fileStore.TargetFolder.Name,
                    _appUser.UserName);

            }
            else if (fileStore.CleanTargetFolder)
            {
                if (!fileStore.TargetFolder.PurgeContents())
                {
                    Log.Error("Could not purge the contents in {UserName} staging folder {SubFolderName}. Path: {DistroPath}",
                        _appUser.UserName,
                        fileStore.TargetFolder.Name,
                        fileStore.TargetFolder.FullName);
            
                    return false;
                }
                else Log.Debug("Purged the contents in {UserName} staging folder {SubFolderName}",
                    _appUser.UserName,
                    fileStore.TargetFolder.Name);
            }

            HandleFileStore(fileStore);
        }
        
        Log.Debug("Leaving the phase {PhaseName} of the task {TaskName}", nameof(Execute), Name);
        return true;
    }
    
    public bool Finish()
    {
        Log.Debug("Entering the phase {PhaseName} of the task {TaskName}", nameof(Finish), Name);

        if (!_targetBundle.TransformAndProvisionAllFiles())
        {
            Log.Error("There were errors copying and transforming the files. See the log for details");
            return false;
        }

        if (_appUser.TargetFolder is null)
            throw new NullReferenceException($"{nameof(_appUser.TargetFolder)} should not be null at this point. Initial checks missing?");
        
        var installScriptPath = new FileInfo(Path.Combine(_appUser.TargetFolder.FullName, $"deploy2{_appUser.UserName}"));
        
        if (!WriteInstallScript(installScriptPath, _appUser.UserName, _targetBundle.GetGroupedTargetFilesForFolder()))
        {
            Log.Error("There were errors creating the install script. See the log for details");
            return false;
        }
        
        Log.Debug("Leaving the phase {PhaseName} of the task {TaskName}", nameof(Finish), Name);
        return true;
    }
    
    private bool HandleFileStore(IAppUserFileStore fileStore)
    {
        if (fileStore.TargetFolder is null)
            throw new NullReferenceException($"{nameof(fileStore.TargetFolder)} should not be null at this point. Initial checks missing?");
        
        Guard.Against.InvalidInput(fileStore, nameof(fileStore), fs => fs.SourceFolder.Exists && fs.TargetFolder is not null && fs.TargetFolder.Exists);

        foreach (var file in fileStore.SourceFolder.EnumerateFiles())
        {
            var fileName = file.Name;
            var nameParseResult = _nameParser.Parse(fileName, _appUser.ActiveTags);

            if (!nameParseResult.Success)
            {
                Log.Error("Could not parse the file name {FileName} in the {UserName} staging folder {SubFolderName}. Path: {FilePath}",
                    file.Name,
                    _appUser.UserName,
                    fileStore.TargetFolder.Name,
                    file.FullName);
            
                return false;
            }
            else if (nameParseResult.DoIgnore)
            {
                Log.Debug("The file {FileName} is not in scope for deployment to {UserName} on {StagingEnvironment}",
                    file.Name,
                    _appUser.UserName,
                    _config.StagingEnvironment.Key);
                
                continue;
            }
            else if (nameParseResult.DoRename)
            {
                Log.Debug("The file name {FileName} contained tags and will be renamed to {NewName}",
                    file.Name,
                    nameParseResult.NewName);
                
                if (nameParseResult.Tags.Any())
                    Log.Debug("The file name {FileName} contained additional tags: {AdditionalTags}",
                        file.Name,
                        nameParseResult.Tags);

                fileName = nameParseResult.NewName;
            }
            
            var rule = GetMatchingTransferRule(fileName, fileStore.Rules);

            if (rule.IsDenyRule)
            {
                Log.Debug("The file {FileName} will be skipped", file.Name);
                continue;
            }

            if (!rule.IsCatchAll && !string.IsNullOrWhiteSpace(rule.NameReplacement))
            {
                fileName = rule.IsPattern
                    ? rule.RexPattern?.Replace(fileName, rule.NameReplacement) ??
                      throw new Exception("When a transfer rule matches a pattern, it must supply a RegEx instance")
                    : rule.NameReplacement;
            }

            var targetFile = new FileInfo(Path.Combine(fileStore.TargetFolder.FullName, fileName));

            _targetBundle.AppendFromUserDeployment(file, targetFile, fileStore.GroupName, rule, nameParseResult.Tags);
        }

        return true;
    }

    private static ITransferRule GetMatchingTransferRule(string fileName, IEnumerable<ITransferRule> rules)
    {
        foreach (var rule in rules)
        {
            var isMatch = rule.IsPattern
                ? rule.RexPattern?.IsMatch(fileName) ??
                  throw new Exception("When a transfer rule matches a pattern, it must supply a RegEx instance")
                : rule.NameToMatch.Equals(fileName);
            
            if (isMatch)
                return rule;
        }

        throw new Exception($"Could not find a transfer rule matching the file name {fileName}");
    }
    
    private readonly TargetBundle _targetBundle = new();
    private readonly IAppUser _appUser;
    private readonly IVigoConfig _config;
    private readonly INameParser _nameParser;
    
    private static bool WriteInstallScript(FileInfo installScript, string userName, IReadOnlyDictionary<TargetFileForFolderGroupKey, IReadOnlyCollection<TargetFileForFolder>> groupedFiles)
    {
        return true;
    }
}