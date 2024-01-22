using Ardalis.GuardClauses;
using Serilog;

namespace vigoftg;

internal class NameParser : INameParser
{
    internal NameParser(Configuration configuration)
    {
        _configuration = configuration;
    }
    
    public INameParseResult Parse(string name, params string[] activeTags)
    {
        return ParseFileName(name, activeTags);
    }

    public INameParseResult Parse(string name, IEnumerable<string> activeTags)
    {
        return ParseFileName(name, activeTags);
    }

    private INameParseResult ParseFileName(string name, IEnumerable<string> activeTags)
    {
        var sanitizedFileName = Guard.Against.NullOrWhiteSpace(name).Trim();
        var activeTagsDict = new Dictionary<string, string>(StringComparer.Ordinal);
        
        foreach (var tag in activeTags)
        {
            var (ok, tagname) = _configuration.CheckTagExistenceAndGetTagName(tag);
            if (!ok || tagname is null)
            {
                Log.Error("The active tag {TagName} is not found in the list of available tags. Did you miss initialization of the parser?", tag);
                activeTagsDict.Clear();
                return new NameParseResult(_configuration, sanitizedFileName, activeTagsDict);
            }
            activeTagsDict.Add(tagname, tagname);
        }

        return new NameParseResult(_configuration, sanitizedFileName, activeTagsDict);
    }

    private readonly Configuration _configuration;
}