using Ardalis.GuardClauses;
using Serilog;

namespace vigoftg;

internal class Configuration
{
    internal (bool, string?) CheckTagExistenceAndGetTagName(string tag)
    {
        var trimmedTag = Guard.Against.NullOrWhiteSpace(tag).Trim();

        if (_caseInsensitiveTags.TryGetValue(trimmedTag, out var tagname))
            return (true, tagname);
        
        return _caseSensitiveTags.TryGetValue(trimmedTag, out tagname) 
            ? (true, tagname) 
            : (false, null);
    }
    
    internal bool AddTag(bool isCaseSensitive, string tag)
    {
        var trimmedTag = Guard.Against.NullOrWhiteSpace(tag).Trim();
		
        if (_caseSensitiveTags.ContainsKey(trimmedTag) ||
            _caseInsensitiveTags.ContainsKey(trimmedTag))
        {
            Log.Error("Failed to add the case {Sensitivity} tag {TagName}. There is already another tag with the same name", 
                isCaseSensitive ? "sensitive" : "insensitive",
                tag);
            return false;
        }
		
        Log.Debug("Adding the case {Sensitivity} tag {TagName} for file name parsing", 
            isCaseSensitive ? "sensitive" : "insensitive",
            trimmedTag);
        
        if (isCaseSensitive)
            _caseSensitiveTags.Add(trimmedTag, trimmedTag);
        else 
            _caseInsensitiveTags.Add(trimmedTag, trimmedTag);

        return true;
    }

    internal Configuration()
    {
        _caseSensitiveTags = new Dictionary<string, string>(StringComparer.Ordinal);
        _caseInsensitiveTags = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    }
	
    private readonly Dictionary<string, string> _caseSensitiveTags;
    private readonly Dictionary<string, string> _caseInsensitiveTags;
}