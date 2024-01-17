namespace vigocfg;

public interface INameParserConfig
{
    IEnumerable<string> CaseSensitiveFilterTags { get; }    
    IEnumerable<string> CaseInsensitiveFilterTags { get; }    
}