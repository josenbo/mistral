using Ardalis.GuardClauses;
using JetBrains.Annotations;
using Serilog;

namespace vigoftg;

[PublicAPI]
public class NameParser : INameParser
{
    public bool AddCaseSensitiveTag(string tag)
    {
        return AddTag(true, tag);
    }

    public bool AddCaseSensitiveTags(IEnumerable<string> tags)
    {
        return AddTags(true, tags);
    }

    public bool AddCaseInsensitiveTag(string tag)
    {
        return AddTag(false, tag);	
    }

    public bool AddCaseInsensitiveTags(IEnumerable<string> tags)
    {
        return AddTags(false, tags);
    }

    public bool AddTag(bool isCaseSensitive, string tag)
    {
        return _configuration.AddTag(isCaseSensitive, tag);
    }

    public bool AddTags(bool isCaseSensitive, IEnumerable<string> tags)
    {
        return tags.All(tag => _configuration.AddTag(isCaseSensitive, tag));
    }

    public INameParseResult ParseFileName(string name, IEnumerable<string> activeTags)
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
    
    INameParseResult INameParser.Parse(string name, params string[] activeTags)
    {
        return ParseFileName(name, activeTags);
    }

    INameParseResult INameParser.Parse(string name, IEnumerable<string> activeTags)
    {
        return ParseFileName(name, activeTags);
    }

    private readonly Configuration _configuration = new();
}