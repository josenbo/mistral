namespace vigocfg;

internal static class EnvironmentHelper
{
    internal static StagingEnvironmentEnum Staging
    {
        get => _staging;
        set => SetStaging(value);
    }

    internal static LineEndingEnum DefaultLineEnding { get; set; }
    
    // ReSharper disable once MemberCanBePrivate.Global
    internal static LineEndingEnum PlatformLineEnding => Environment.OSVersion.Platform == PlatformID.Unix
                                                            ? LineEndingEnum.LF
                                                            : LineEndingEnum.CR_LF;

    internal static IEnumerable<string> StagingTags => ListOfStagingTags;

    static EnvironmentHelper()
    {
        DefaultLineEnding = PlatformLineEnding;
    }

    private static void SetStaging(StagingEnvironmentEnum stagingEnvironment)
    {
        ListOfStagingTags.Clear();
        
        switch (stagingEnvironment)
        {
            case StagingEnvironmentEnum.UserAcceptanceTest:
                ListOfStagingTags.Add("ALL");
                ListOfStagingTags.Add("UAT");
                ListOfStagingTags.Add("NON-PROD");
                ListOfStagingTags.Add("NONPROD");
                break;

            case StagingEnvironmentEnum.Development:
                ListOfStagingTags.Add("ALL");
                ListOfStagingTags.Add("DEV");
                ListOfStagingTags.Add("NON-PROD");
                ListOfStagingTags.Add("NONPROD");
                break;

            case StagingEnvironmentEnum.ContinuousIntegrationAndTest:
                ListOfStagingTags.Add("ALL");
                ListOfStagingTags.Add("CID");
                ListOfStagingTags.Add("NON-PROD");
                ListOfStagingTags.Add("NONPROD");
                break;

            case StagingEnvironmentEnum.Reference:
                ListOfStagingTags.Add("ALL");
                ListOfStagingTags.Add("REF");
                ListOfStagingTags.Add("NON-PROD");
                ListOfStagingTags.Add("NONPROD");
                break;

            case StagingEnvironmentEnum.Production:
                ListOfStagingTags.Add("ALL");
                ListOfStagingTags.Add("PROD");
                break;

            case StagingEnvironmentEnum.Undefined:
            default:
                throw new ArgumentOutOfRangeException(nameof(stagingEnvironment), $"The staging environment {stagingEnvironment} is not handled");
        }

        _staging = stagingEnvironment;
    }

    private static readonly List<string> ListOfStagingTags = new();
    private static StagingEnvironmentEnum _staging = StagingEnvironmentEnum.Undefined;
}