using Serilog;

namespace vigocfg;

internal static class GuessSessionEnvironment
{
    internal static bool GuessAvailable { get; private set; }
    internal static Configuration? GuessedEnvironmentSettings { get; }
    
    static GuessSessionEnvironment()
    {
        Log.Debug("Environment.MachineName = {MachineName}", 
            Environment.MachineName);
        Log.Debug("Environment.OSVersion = {OSVersion}", 
            Environment.OSVersion);
        Log.Debug("Environment.UserDomainName = {UserDomainName}", 
            Environment.UserDomainName);
        Log.Debug("Environment.UserName = {UserName}", 
            Environment.UserName);
        Log.Debug("Environment.Home = {Home}", 
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        var guessedSessionEnvironment = GetSession();
        
        Log.Debug("{ClassName}.{PropertyName} = {PropertyValue}",
            nameof(GuessSessionEnvironment),
            nameof(guessedSessionEnvironment),
            guessedSessionEnvironment);

        GuessAvailable = (guessedSessionEnvironment != GuessSessionEnvironmentEnum.Undefined);
        
        GuessedEnvironmentSettings = guessedSessionEnvironment switch
        {
            GuessSessionEnvironmentEnum.RobotUatOnD9004 => new Configuration(
                StagingEnvironment: StagingEnvironmentEnum.UserAcceptanceTest, IsLegacyHost: false,
                DefaultLineEndingOrNullForPlatformDefault: null,
                SourceRepositoryRoot: new DirectoryInfo("/home/dpagyp/robots/robot_uat/repos/ines_avanti/"),
                SourceRepositoryFlywaySubfolders: new[] { "database", "flyway" },
                SourceRepositoryCupinesSubfolders: new[] { "linux", "cupines" },
                SourceRepositoryStammausSubfolders: new[] { "linux", "stammaus" },
                TargetFolderFlyway: new DirectoryInfo("/home/dpagyp/robots/robot_uat/work/env_uat/flyway"),
                TargetFolderCupines: new DirectoryInfo("/home/dpagyp/robots/shared/deployment/env_uat/cupines/"),
                TargetFolderStammaus: new DirectoryInfo("/home/dpagyp/robots/shared/deployment/env_uat/stammaus/")),
            
            GuessSessionEnvironmentEnum.DhlNotebookJochen => new Configuration(
                StagingEnvironment: StagingEnvironmentEnum.UserAcceptanceTest, IsLegacyHost: false,
                DefaultLineEndingOrNullForPlatformDefault: null,
                SourceRepositoryRoot: new DirectoryInfo(@"I:\INES-Github\ines_avanti"),
                SourceRepositoryFlywaySubfolders: new[] { "database", "flyway" },
                SourceRepositoryCupinesSubfolders: new[] { "linux", "cupines" },
                SourceRepositoryStammausSubfolders: new[] { "linux", "stammaus" },
                TargetFolderFlyway: new DirectoryInfo(@"I:\INES-Github\JSN\PipelineTools\env\env_uat\flyway"),
                TargetFolderCupines: new DirectoryInfo(@"I:\INES-Github\JSN\PipelineTools\env\env_uat\cupines"),
                TargetFolderStammaus: new DirectoryInfo(@"I:\INES-Github\JSN\PipelineTools\env\env_uat\stammaus")),
            
            _ => null
        };
    }
    
    private static GuessSessionEnvironmentEnum GetSession()
    {
        if (Environment.MachineName == "DEBONWN8301893" &&
            Environment.UserDomainName == "PRG-DC" &&
            Environment.UserName == "b7o1tue0137" &&
            Environment.OSVersion.Platform == PlatformID.Win32NT)
            
            return GuessSessionEnvironmentEnum.DhlNotebookJochen;
            
        if (Environment.MachineName == "INES-D9004" &&
            Environment.UserName == "robot_uat" &&
            Environment.OSVersion.Platform == PlatformID.Unix)

            return GuessSessionEnvironmentEnum.RobotUatOnD9004;
        
        return GuessSessionEnvironmentEnum.Undefined;
    }
}