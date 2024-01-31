using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace vigobase;

[PublicAPI]
public static class FileTypeEnumHelper
{
    public static bool IsDefined(int value) => Enum.IsDefined(typeof(FileTypeEnum), value);
    public static bool IsDefined(this FileTypeEnum value) => Enum.IsDefined(typeof(FileTypeEnum), value);
    public static bool IsDefinedAndValid(int value) => Enum.IsDefined(typeof(FileTypeEnum), value) && (FileTypeEnum)value != FileTypeEnum.Undefined;
    public static bool IsDefinedAndValid(this FileTypeEnum value) => Enum.IsDefined(typeof(FileTypeEnum), value) && value != FileTypeEnum.Undefined;
    public static bool IsTextFile(this FileTypeEnum value) => value == FileTypeEnum.TextFile;
    public static bool IsBinaryFile(this FileTypeEnum value) => value == FileTypeEnum.BinaryFile;
    public static bool TryParse(string? text, [NotNullWhen(true)] out FileTypeEnum? result)
    {
        result = (text?.Trim().ToLower() ?? string.Empty) switch
        {
            "bin" => FileTypeEnum.BinaryFile,
            "binary" => FileTypeEnum.BinaryFile,
            "txt" => FileTypeEnum.TextFile,
            "text" => FileTypeEnum.TextFile,
            _ => null
        };
        return result.HasValue;
    }
}