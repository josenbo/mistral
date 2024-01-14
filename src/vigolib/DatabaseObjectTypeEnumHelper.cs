namespace vigolib;

internal static class DatabaseObjectTypeEnumHelper
{
    internal static bool IsDefined(int value) => Enum.IsDefined(typeof(DatabaseObjectTypeEnum), value);
    internal static bool IsDefined(this DatabaseObjectTypeEnum value) => Enum.IsDefined(typeof(DatabaseObjectTypeEnum), value);
    internal static bool IsDefinedAndValid(int value) => Enum.IsDefined(typeof(DatabaseObjectTypeEnum), value) && (DatabaseObjectTypeEnum)value != DatabaseObjectTypeEnum.Undefined;
    internal static bool IsDefinedAndValid(this DatabaseObjectTypeEnum value) => Enum.IsDefined(typeof(DatabaseObjectTypeEnum), value) && value != DatabaseObjectTypeEnum.Undefined;
}