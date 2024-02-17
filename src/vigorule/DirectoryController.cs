using JetBrains.Annotations;
using Serilog;
using vigobase;

namespace vigoconfig;

[PublicAPI]
public class DirectoryController : IFolderConfiguration
{
    public FileHandlingParameters ParentFileHandlingParams { get; }
    public FileHandlingParameters LocalFileHandlingParams { get; private set; }
    public DirectoryInfo Location { get; }

    public IDeploymentTransformationReadWriteDirectory GetDirectoryTransformation()
    {
        return new DeploymentTransformationDirectory(Location, _keepEmptyDirector);
    }
    
    public IDeploymentTransformationReadWriteFile GetFileTransformation(FileInfo file)
    {
        foreach (var rule in _rules)
        {
            if (rule.GetTransformation(file, out var transformation))
                return transformation;
        }
        
        Log.Fatal("There is no rule matching the file name {TheFileName} in the directory {TheDirectory}",
            file.Name,
            LocalFileHandlingParams.Settings.GetRepoRelativePath(file.FullName));
        throw new VigoFatalException("Could not find a file rule");
    }
    
    public DirectoryController(DirectoryInfo location, FileHandlingParameters parentFileHandlingParams)
    {
        LocalFileHandlingParams = ParentFileHandlingParams = parentFileHandlingParams;
        Location = location;
        FolderConfigurator.Configure(this);
    }

    private readonly List<FileRule> _rules = [];
    private bool _keepEmptyDirector;

    #region interface IFolderConfiguration

    int IFolderConfiguration.NextRuleIndex => _rules.Count;

    void IFolderConfiguration.SetLocalDefaults(FileHandlingParameters localDefaults)
    {
        LocalFileHandlingParams = localDefaults;
    }

    bool IFolderConfiguration.HasKeepFolderFlag
    {
        get => _keepEmptyDirector;
        set => _keepEmptyDirector = value;
    }

    void IFolderConfiguration.AddRule(FileRule rule)
    {
        if (rule.Id.Index != _rules.Count)
        {
            const string message = "Precondition failed for rule numbering"; 
            Log.Fatal(message);
            throw new VigoFatalException(message);
        }
        _rules.Add(rule);
    }
   
    #endregion
}
