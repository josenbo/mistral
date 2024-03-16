namespace vigobase;

public static class ConfigurationFileTypeEnumHelper
{
    public static bool IsDefined(int value) => Enum.IsDefined(typeof(ConfigurationFileTypeEnum), value);
    public static bool IsDefined(this ConfigurationFileTypeEnum value) => Enum.IsDefined(typeof(ConfigurationFileTypeEnum), value);
    public static bool IsDefinedAndValid(int value) => Enum.IsDefined(typeof(ConfigurationFileTypeEnum), value) && (ConfigurationFileTypeEnum)value != ConfigurationFileTypeEnum.Undefined;
    public static bool IsDefinedAndValid(this ConfigurationFileTypeEnum value) => Enum.IsDefined(typeof(ConfigurationFileTypeEnum), value) && value != ConfigurationFileTypeEnum.Undefined;
    public static bool IsMarkdownFormat(this ConfigurationFileTypeEnum value) => value == ConfigurationFileTypeEnum.MarkdownFormat;
    public static bool IsNativeFormat(this ConfigurationFileTypeEnum value) => value == ConfigurationFileTypeEnum.NativeFormat;
}