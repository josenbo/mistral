namespace vigobase;

public record AppEnvValues(
    CommandEnum Command, 
    DirectoryInfo TopLevelDirectory,
    FileHandlingParameters DefaultFileHandlingParams,
    StandardFileHandling DeployConfigRule,
    StandardFileHandling FinalCatchAllRule,
    DirectoryInfo TempDirectory
);