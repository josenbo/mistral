using JetBrains.Annotations;

namespace vigocfg;

[PublicAPI]
public static class FileTypeEnumHelper
{
    public static bool IsDefined(int value) => Enum.IsDefined(typeof(FileTypeEnum), value);
    public static bool IsDefined(this FileTypeEnum value) => Enum.IsDefined(typeof(FileTypeEnum), value);
    public static bool IsDefinedAndValid(int value) => Enum.IsDefined(typeof(FileTypeEnum), value) && (FileTypeEnum)value != FileTypeEnum.Undefined;
    public static bool IsDefinedAndValid(this FileTypeEnum value) => Enum.IsDefined(typeof(FileTypeEnum), value) && value != FileTypeEnum.Undefined;
}