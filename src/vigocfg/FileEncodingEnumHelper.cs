using System.Text;
using JetBrains.Annotations;

namespace vigocfg;

[PublicAPI]
public static class FileEncodingEnumHelper
{
    public static bool IsDefined(int value) => Enum.IsDefined(typeof(FileEncodingEnum), value);
    public static bool IsDefined(this FileEncodingEnum value) => Enum.IsDefined(typeof(FileEncodingEnum), value);
    public static Encoding ToEncoding(int value) => ((FileEncodingEnum)value).ToEncoding();
    public static bool IsDefinedAndValid(int value) => Enum.IsDefined(typeof(FileEncodingEnum), value) && (FileEncodingEnum)value != FileEncodingEnum.Undefined;
    public static bool IsDefinedAndValid(this FileEncodingEnum value) => Enum.IsDefined(typeof(FileEncodingEnum), value) && value != FileEncodingEnum.Undefined;
    public static Encoding ToEncoding(this FileEncodingEnum value)
    {
        // Add a reference to the System.Text.Encoding.CodePages package 
        // then register the provider to make ISO-8859-15 available
        var instance = CodePagesEncodingProvider.Instance;
        Encoding.RegisterProvider(instance);

        return value switch
        {
            FileEncodingEnum.Undefined => throw new ArgumentOutOfRangeException(nameof(value), value, "Cannot fetch an encoding object for an undefined value"),
            FileEncodingEnum.ISO_8859_1 => Encoding.GetEncoding("ISO-8859-1"),
            FileEncodingEnum.Ascii => Encoding.ASCII,
            FileEncodingEnum.UTF_8 => new UTF8Encoding(false),
            FileEncodingEnum.ISO_8859_15 => Encoding.GetEncoding("ISO-8859-15"),
            FileEncodingEnum.Windows_1252 => Encoding.GetEncoding(1252),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, $"Do not know how to handle the encoding {value}")
        };
    }
}