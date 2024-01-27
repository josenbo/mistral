﻿using vigoarchive;
using vigobase;
using vigoconfig;

namespace vigo;

internal class TarballBuilder
{
    public void Build()
    {
        var tarball = new Tarball(_configuration.RepositoryRoot.FullName, _configuration.AdditionalTarRootFolder);
        var defaults = GetDeploymentDefaults(_configuration.DeploymentConfigFileName);
        var rules = new DirectoryDeploymentController(_configuration.RepositoryRoot, defaults);

        ProcessDirectory(rules, tarball);
        
        tarball.Save(_configuration.Tarball);
    }
    
    internal TarballBuilder(Configuration configuration)
    {
        _configuration = configuration;
    }

    private static void ProcessDirectory(DirectoryDeploymentController controller, Tarball tarball)
    {
        if (controller.HasDeploymentRules)
        {
            foreach (var fi in controller.Location.EnumerateFiles())
            {
                if (controller.DeployFile(fi))
                    tarball.AddFile(fi);
            }
        }

        foreach (var di in controller.Location.EnumerateDirectories())
        {
            if (di.Name.Equals(".git", StringComparison.InvariantCultureIgnoreCase))
                continue;
            ProcessDirectory(new DirectoryDeploymentController(di, controller.Defaults), tarball);
        }
    }
    
    private static DeploymentDefaults GetDeploymentDefaults(string deploymentConfigFileName)
    {
        return new DeploymentDefaults(
            DeploymentConfigFileName: deploymentConfigFileName,
            FileModeDefault: (UnixFileMode)0b_110_110_100,
            DirectoryModeDefault: (UnixFileMode)0b_111_111_101,
            SourceFileEncodingDefault: FileEncodingEnum.UTF_8,
            TargetFileEncodingDefault: FileEncodingEnum.UTF_8,
            LineEndingDefault: LineEndingEnum.LF,
            TrailingNewlineDefault: true,
            FileTypeDefault: FileTypeEnum.TextFile    
        );
    }

    private readonly Configuration _configuration;
}