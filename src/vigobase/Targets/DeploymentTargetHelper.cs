using System.Text.RegularExpressions;
using Serilog;

namespace vigobase;

public static partial class DeploymentTargetHelper
{
    public static IEnumerable<string> ParseTargets(string? targets)
    {
        if (string.IsNullOrWhiteSpace(targets))
            yield break;

        var dict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);                

        foreach (var name in  targets.Split(
                     Separators,
                     StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                 ).Distinct(StringComparer.InvariantCultureIgnoreCase))
        {
            if (!RexTargetName.IsMatch(name))
            {
                Log.Fatal("Invalid target name {TheTargetName}", name);
                throw new VigoFatalException(AppEnv.Faults.Fatal("FX518","Invalid target name"));
            }

            var knownName = GetOrRegisterKnownName(name);

            if (!dict.TryAdd(knownName, knownName))
                continue;

            yield return knownName;
        }
    }
    
    public static bool IsValidName(string targetName)
    {
        return RexTargetName.IsMatch(targetName);
    }

    private static string GetOrRegisterKnownName(string name)
    {
        if (KnownTargets.TryGetValue(name, out var knownName))
            return knownName;
        KnownTargets.Add(name, name);
        return name;
    }

    static DeploymentTargetHelper()
    {
        GetOrRegisterKnownName("Prod");
        GetOrRegisterKnownName("Production");
        GetOrRegisterKnownName("NonProd");
        GetOrRegisterKnownName("NonProduction");
        GetOrRegisterKnownName("Non-Production");
    }

    private static readonly Dictionary<string, string> KnownTargets =
        new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase); 
    private static readonly char[] Separators = new char[] {' ', ',', ';'};
    private static readonly Regex RexTargetName = CompiledRexTargetName();

    [GeneratedRegex("^[a-zA-Z]([-_.]?[a-zA-Z0-9]{1,40}){1,40}$")]
    private static partial Regex CompiledRexTargetName();
}