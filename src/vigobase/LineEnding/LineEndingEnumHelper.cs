using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace vigobase;

[PublicAPI]
public static class LineEndingEnumHelper
{
    public static bool IsDefined(int value) => Enum.IsDefined(typeof(LineEndingEnum), value);
    public static bool IsDefined(this LineEndingEnum value) => Enum.IsDefined(typeof(LineEndingEnum), value);
    public static bool IsDefinedAndValid(int value) => Enum.IsDefined(typeof(LineEndingEnum), value) && (LineEndingEnum)value != LineEndingEnum.Undefined;
    public static bool IsDefinedAndValid(this LineEndingEnum value) => Enum.IsDefined(typeof(LineEndingEnum), value) && value != LineEndingEnum.Undefined;

    public static string ToNewlineSequence(this LineEndingEnum value)
    {
        return value switch
        {
            LineEndingEnum.Undefined => throw new ArgumentOutOfRangeException(nameof(value), value, "Cannot return a newline sequence for an undefined value"),
            LineEndingEnum.LF => "\n",
            LineEndingEnum.CR_LF => "\r\n",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, $"Unknown line ending {value} cannot be converted into a newline sequence")
        };
    }
    
    public static bool TryParse(string text, [NotNullWhen(true)] out LineEndingEnum? result)
    {
        result = text
                .Replace("_", "")
                .Replace("-", "")
                .Replace(" ", "")
                .Replace(".", "")
                .Trim()
                .ToLower() switch
            {
                "crlf" => LineEndingEnum.CR_LF,
                "windows" => LineEndingEnum.CR_LF,
                "win" => LineEndingEnum.CR_LF,
                "lf" => LineEndingEnum.LF,
                "unix" => LineEndingEnum.LF,
                "linux" => LineEndingEnum.LF,
                _ => null
            };
        return result.HasValue;
    }
}