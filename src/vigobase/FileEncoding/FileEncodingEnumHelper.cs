using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;

namespace vigobase;

[PublicAPI]
public static class FileEncodingEnumHelper
{
    public static bool IsDefined(int value) => Enum.IsDefined(typeof(FileEncodingEnum), value);
    public static bool IsDefined(this FileEncodingEnum value) => Enum.IsDefined(typeof(FileEncodingEnum), value);
    public static System.Text.Encoding ToEncoding(int value) => ((FileEncodingEnum)value).ToEncoding();
    public static bool IsDefinedAndValid(int value) => Enum.IsDefined(typeof(FileEncodingEnum), value) && (FileEncodingEnum)value != FileEncodingEnum.Undefined;
    public static bool IsDefinedAndValid(this FileEncodingEnum value) => Enum.IsDefined(typeof(FileEncodingEnum), value) && value != FileEncodingEnum.Undefined;
    public static System.Text.Encoding ToEncoding(this FileEncodingEnum value)
    {
        // Add a reference to the System.Text.Encoding.CodePages package 
        // then register the provider to make ISO-8859-15 available
        var instance = CodePagesEncodingProvider.Instance;
        System.Text.Encoding.RegisterProvider(instance);

        return value switch
        {
            FileEncodingEnum.Undefined => throw new ArgumentOutOfRangeException(nameof(value), value, "Cannot fetch an encoding object for an undefined value"),
            FileEncodingEnum.ISO_8859_1 => System.Text.Encoding.GetEncoding("ISO-8859-1"),
            FileEncodingEnum.Ascii => System.Text.Encoding.ASCII,
            FileEncodingEnum.UTF_8 => new UTF8Encoding(false),
            FileEncodingEnum.ISO_8859_15 => System.Text.Encoding.GetEncoding("ISO-8859-15"),
            FileEncodingEnum.Windows_1252 => System.Text.Encoding.GetEncoding(1252),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, $"Do not know how to handle the encoding {value}")
        };
    }
    public static bool TryParse(string? text, [NotNullWhen(true)] out FileEncodingEnum? result)
    {
        result = (text ?? string.Empty)
                .Replace("_", "")
                .Replace("-", "")
                .Replace(" ", "")
                .Replace(".", "")
                .Trim()
                .ToLower() switch
        {
            "utf8" => FileEncodingEnum.UTF_8,
            "ascii" => FileEncodingEnum.Ascii,
            "iso88591" => FileEncodingEnum.ISO_8859_1,
            "iso885915" => FileEncodingEnum.ISO_8859_15,
            "windows1252" => FileEncodingEnum.Windows_1252,
            "win1252" => FileEncodingEnum.Windows_1252,
            _ => null
        };
        return result.HasValue;
    }
}