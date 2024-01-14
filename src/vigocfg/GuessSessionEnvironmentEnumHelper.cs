namespace vigocfg;

internal static class GuessSessionEnvironmentEnumHelper
{
    internal static bool IsDefined(int value) => Enum.IsDefined(typeof(GuessSessionEnvironmentEnum), value);
    internal static bool IsDefined(this GuessSessionEnvironmentEnum value) => Enum.IsDefined(typeof(GuessSessionEnvironmentEnum), value);
    internal static bool IsDefinedAndValid(int value) => Enum.IsDefined(typeof(GuessSessionEnvironmentEnum), value) && (GuessSessionEnvironmentEnum)value != GuessSessionEnvironmentEnum.Undefined;
    internal static bool IsDefinedAndValid(this GuessSessionEnvironmentEnum value) => Enum.IsDefined(typeof(GuessSessionEnvironmentEnum), value) && value != GuessSessionEnvironmentEnum.Undefined;
}