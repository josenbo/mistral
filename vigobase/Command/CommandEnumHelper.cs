namespace vigobase;

public static class CommandEnumHelper
{
    public static bool IsDefined(int value) => Enum.IsDefined(typeof(FileTypeEnum), value);
    public static bool IsDefined(this CommandEnum value) => Enum.IsDefined(typeof(CommandEnum), value);
    public static bool IsDefinedAndValid(int value) => Enum.IsDefined(typeof(CommandEnum), value) && (CommandEnum)value != CommandEnum.Undefined;
    public static bool IsDefinedAndValid(this CommandEnum value) => Enum.IsDefined(typeof(CommandEnum), value) && value != CommandEnum.Undefined;
    public static bool IsDeployToTarball(this CommandEnum value) => value == CommandEnum.Deploy;
    public static bool IsCheckCommit(this CommandEnum value) => value == CommandEnum.Check;
    public static CommandEnum Parse(string? command)
    {
        command = command?.Trim().ToLowerInvariant() ?? string.Empty;

        switch (command)
        {
            case "tarball":
                return CommandEnum.Deploy;
            case "check":
                return CommandEnum.Check;
            default:
                var message = $"Unknown command name \"{command}\". Valid names are \"Tarball\" and \"Check\"";
                Console.Error.WriteLine(message);
                throw new ArgumentException(message, nameof(command));
        }
    }
}