namespace vigobase;

public static class CommandEnumHelper
{
    public static bool IsDefined(int value) => Enum.IsDefined(typeof(FileTypeEnum), value);
    public static bool IsDefined(this CommandEnum value) => Enum.IsDefined(typeof(CommandEnum), value);
    public static bool IsDefinedAndValid(int value) => Enum.IsDefined(typeof(CommandEnum), value) && (CommandEnum)value != CommandEnum.Undefined;
    public static bool IsDefinedAndValid(this CommandEnum value) => Enum.IsDefined(typeof(CommandEnum), value) && value != CommandEnum.Undefined;
}