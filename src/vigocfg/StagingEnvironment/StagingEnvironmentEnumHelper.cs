using Ardalis.GuardClauses;

namespace vigocfg;

// public static class StagingEnvironmentEnumHelper
// {
//     public static bool IsDefined(int value) => Enum.IsDefined(typeof(StagingEnvironmentEnum), value);
//     public static bool IsDefined(this StagingEnvironmentEnum value) => Enum.IsDefined(typeof(StagingEnvironmentEnum), value);
//     public static bool IsDefinedAndValid(int value) => Enum.IsDefined(typeof(StagingEnvironmentEnum), value) && (StagingEnvironmentEnum)value != StagingEnvironmentEnum.Undefined;
//     public static bool IsDefinedAndValid(this StagingEnvironmentEnum value) => Enum.IsDefined(typeof(StagingEnvironmentEnum), value) && value != StagingEnvironmentEnum.Undefined;
//     public static string Key (this StagingEnvironmentEnum value) => GetKey(value);
//     public static string Name (this StagingEnvironmentEnum value) => GetName(value);
//
//     public static StagingEnvironmentEnum GetEnumFromKey(string key)
//     {
//         return Guard.Against.NullOrWhiteSpace(key).Trim().ToUpperInvariant() switch
//         {
//             "DEV" => StagingEnvironmentEnum.Development,
//             "CID" => StagingEnvironmentEnum.ContinuousIntegrationAndTest,
//             "UAT" => StagingEnvironmentEnum.UserAcceptanceTest,
//             "REF" => StagingEnvironmentEnum.Reference,
//             "PROD" => StagingEnvironmentEnum.Production,
//             _ => StagingEnvironmentEnum.Undefined
//         };
//     }
//     
//     private static string GetKey(StagingEnvironmentEnum value)
//     {
//         return value switch
//         {
//             StagingEnvironmentEnum.Undefined => "???",
//             StagingEnvironmentEnum.Development => "DEV",
//             StagingEnvironmentEnum.ContinuousIntegrationAndTest => "CID",
//             StagingEnvironmentEnum.UserAcceptanceTest => "UAT",
//             StagingEnvironmentEnum.Reference => "REF",
//             StagingEnvironmentEnum.Production => "PROD",
//             _ => "???"
//         };
//     }
//     
//     private static string GetName(StagingEnvironmentEnum value)
//     {
//         return value switch
//         {
//             StagingEnvironmentEnum.Undefined => "(nicht festgelegt)",
//             StagingEnvironmentEnum.Development => "Development",
//             StagingEnvironmentEnum.ContinuousIntegrationAndTest => "Continuous Integration and Test",
//             StagingEnvironmentEnum.UserAcceptanceTest => "User Acceptance Test",
//             StagingEnvironmentEnum.Reference => "Reference",
//             StagingEnvironmentEnum.Production => "Production",
//             _ => "(unbekannt)"
//         };
//     }
// }