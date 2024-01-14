namespace vigocfg;

internal static class LocalStringExtensions
{
    public static string[] PrependToStringArray(this string value, IEnumerable<string> parts)
    {
        var list = new List<string>(){value};
        list.AddRange(parts);
        return list.ToArray();
    }

    public static string[] PrependToStringArray(this string value, params string[] parts)
    {
        var list = new List<string>() { value };
        list.AddRange(parts);
        return list.ToArray();
    }
}