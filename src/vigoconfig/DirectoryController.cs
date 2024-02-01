﻿using JetBrains.Annotations;
using Serilog;
using vigobase;

namespace vigoconfig;

[PublicAPI]
public class DirectoryController : IFolderConfiguration
{
    public DeploymentDefaults Defaults { get; }
    public DirectoryInfo Location { get; }

    public IDeploymentTransformationReadWriteDirectory GetDirectoryTransformation()
    {
        return new DeploymentTransformationDirectory(Location, _keepEmptyDirector);
    }
    
    public IDeploymentTransformationReadWriteFile GetFileTransformation(FileInfo file)
    {
        foreach (var rule in _rules)
        {
            if (rule.GetTransformation(file, Defaults, out var transformation))
                return transformation;
        }
        
        Log.Fatal("There is no rule matching the file name {TheFileName} in the directory {TheDirectory}",
            file.Name,
            Defaults.GetRepositoryRelativePath(file.FullName));
        throw new VigoFatalException("Could not find a file rule");
    }
    
    public DirectoryController(DirectoryInfo directory, DeploymentDefaults defaults)
    {
        Defaults = defaults;
        Location = directory;
        FolderConfigurator.Configure(this);
    }

    private readonly List<Rule> _rules = [];
    private bool _keepEmptyDirector;

    #region interface IFolderConfiguration

    int IFolderConfiguration.NextRuleIndex => _rules.Count;

    bool IFolderConfiguration.HasKeepFolderFlag
    {
        get => _keepEmptyDirector;
        set => _keepEmptyDirector = value;
    }

    void IFolderConfiguration.AddRule(Rule rule)
    {
        if (rule.Index != _rules.Count)
        {
            const string message = "Precondition failed for rule numbering"; 
            Log.Fatal(message);
            throw new VigoFatalException(message);
        }
        _rules.Add(rule);
    }
   
    #endregion
}
