using Serilog;

namespace vigoftg;

internal class NameParseResult : INameParseResult
{
	// ReSharper disable once RedundantDefaultMemberInitializer
	public bool Success { get; private set; } = false;
	public bool DoIgnore { get; private set; } = true;

	// ReSharper disable once RedundantDefaultMemberInitializer
	public bool DoRename { get; private set; } = false;
	public string CurrentName { get; }
	public string NewName { get; private set; } = string.Empty;

	public IEnumerable<string> Tags => _additionalTagsFoundInName.Values;

	public bool HasTag(string tag) => _additionalTagsFoundInName.ContainsKey(tag);

	internal NameParseResult(Configuration configuration, string name, IReadOnlyDictionary<string, string> activeTagsDict)
	{
		_configuration = configuration;
		_activeTagsDict = activeTagsDict;
		CurrentName = name;

		Log.Debug("Parsing the name {Name} with the active tags {ActiveTagList}",
			CurrentName, 
			_activeTagsDict.Values);
		
		Parse();
	}

	private void Parse()
	{
		try
		{
			var parseResult = TaggedNameParser.ParseTaggedName(CurrentName);

			if (!parseResult.HasTags)
			{
				Success = true;
				DoIgnore = false;
				DoRename = false;
				NewName = string.Empty;
				
				Log.Debug("There were no tags in the name {Name}", CurrentName);
				
				return;
			}

			foreach (var additionalTag in parseResult.AdditionalTags)
			{
				_additionalTagsFoundInName.Add(additionalTag, additionalTag);	
			}
			
			if (!parseResult.HasScope)
			{
				Success = true;
				DoIgnore = false;
				DoRename = true;
				NewName = parseResult.NewName;
				
				Log.Debug("There were only additional tags in the name {Name}, but no scope tags. Additional tags = {AdditionalTags}", 
					CurrentName, 
					_additionalTagsFoundInName.Values);
				
				return;
			}
		
			var inScope = parseResult.IsInclusive
				? IsInTagList(parseResult.PrimaryTags)
				: IsNotInTagList(parseResult.PrimaryTags);
	    
			if (parseResult.SecondaryTags.Any())
				inScope = parseResult.IsInclusive
					? IsNotInTagList(parseResult.SecondaryTags)
					: IsInTagList(parseResult.SecondaryTags);

			if (inScope)
			{
				Success = true;
				DoIgnore = false;
				DoRename = true;
				NewName = parseResult.NewName;
				
				Log.Debug("The name {Name} is tagged and will be renamed to {NewName}. The {TagMeaning} tags where {TagsList} with the exception of {ExceptedTagsList}. This matched the active tags {ActiveTagsList}",
					CurrentName,
					NewName,
					parseResult.IsInclusive ? "inclusive" : "exclusive",
					parseResult.PrimaryTags,
					parseResult.SecondaryTags,
					_activeTagsDict.Values
				);
			}
			else
			{
				Success = true;
				DoIgnore = true;
				DoRename = false;
				NewName = string.Empty;
				
				Log.Debug("The name {Name} is tagged but out of scope. The {TagMeaning} tags where {TagsList} with the exception of {ExceptedTagsList}. This did not match the active tags {ActiveTagsList}",
					CurrentName,
					parseResult.IsInclusive ? "inclusive" : "exclusive",
					parseResult.PrimaryTags,
					parseResult.SecondaryTags,
					_activeTagsDict.Values
				);
			}
		}
		catch (NameParserException ex)
		{
			Log.Error(ex, "Failed to parse the name");
			Success = false;
			DoIgnore = true;
			DoRename = false;
			NewName = string.Empty;
		}
	}
	
    private bool IsInTagList(IEnumerable<string> tagList)
    {
	    return tagList.Any(IsActiveTag);
    }
    
    private bool IsNotInTagList(IEnumerable<string> tagList)
    {
	    return tagList.All(e => !IsActiveTag(e));
    }

    private bool IsActiveTag(string tag)
    {
	    var (ok, tagname) = _configuration.CheckTagExistenceAndGetTagName(tag);
	    if (!ok || tagname is null)
		    throw new NameParserException($"Found the unrecognized tag {tag} in the name {CurrentName}");

	    return _activeTagsDict.ContainsKey(tagname);
    }

    private readonly Configuration _configuration;
    private readonly IReadOnlyDictionary<string, string> _activeTagsDict;
    private readonly Dictionary<string, string> _additionalTagsFoundInName = new(StringComparer.InvariantCultureIgnoreCase);
}