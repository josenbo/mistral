namespace vigocfg.Settings;

public record Configuration(
    StagingEnvironmentEnum StagingEnvironment, 
    bool IsLegacyHost, 
    LineEndingEnum? DefaultLineEndingOrNullForPlatformDefault, 
    DirectoryInfo SourceRepositoryRoot,
    string[] SourceRepositoryFlywaySubfolders,
    string[] SourceRepositoryCupinesSubfolders,
    string[] SourceRepositoryStammausSubfolders,
    DirectoryInfo? TargetFolderFlyway,
    DirectoryInfo? TargetFolderCupines,
    DirectoryInfo? TargetFolderStammaus);
    
//  public record EnvironmentSettingsUser