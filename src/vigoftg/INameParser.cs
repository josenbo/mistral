namespace vigoftg;

public interface INameParser
{
    public INameParseResult Parse(string name, params string[] activeTags);
    public INameParseResult Parse(string name, IEnumerable<string> activeTags);
}