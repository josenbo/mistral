using JetBrains.Annotations;

namespace vigobase;

[PublicAPI]
public enum FileTypeEnum
{
    Undefined = 0,
    TextFile = 111001,
    BinaryFile = 111002,
}

// internal static class FileTypeEnumHelper
// {
//     public static bool IsTextFile(this FileTypeEnum value) => value == FileTypeEnum.TextFile;
//     public static bool IsBinaryFile(this FileTypeEnum value) => value == FileTypeEnum.BinaryFile;
//     public static bool IsSet(this FileTypeEnum value) => value is FileTypeEnum.BinaryFile or FileTypeEnum.BinaryFile;
//     public static string AsText(this FileTypeEnum value) => EnumToText(value);
//
//     public static FileTypeEnum TextToEnum(string text)
//     {
//             
//     }
//
//     public static string EnumToText(FileTypeEnum value)
//     {
//         return value switch
//         {
//             FileTypeEnum.TextFile => "text",
//             FileTypeEnum.BinaryFile => "binary",
//             _ => string.Empty
//         };
//     }
// }