namespace vigocfg;

internal class NameParserConfig : INameParserConfig
{
    public IEnumerable<string> CaseSensitiveFilterTags => Array.Empty<string>();

    public IEnumerable<string> CaseInsensitiveFilterTags => new[]
        { "DEV", "CID", "UAT", "REF", "PROD", "NON-PROD", "NONPROD", "ALL", "NONE", "cupines", "stammaus" };
}