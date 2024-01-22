using Serilog;

namespace vigoftg;

internal class NameParseResult : INameParseResult
{
	public bool CanDeploy { get; private set; } = false;
	public string SourceName { get; }
	
	public string TargetName { get; private set; } = string.Empty;

	public IEnumerable<string> Tags => _additionalTagsFoundInName.Values;

	public bool HasTag(string tag) => _additionalTagsFoundInName.ContainsKey(tag);

	internal NameParseResult(Configuration configuration, string name, IReadOnlyDictionary<string, string> activeTagsDict)
	{
		_configuration = configuration;
		_activeTagsDict = activeTagsDict;
		SourceName = name;

		Log.Debug("Parsing the name {Name} with the active tags {ActiveTagList}",
			SourceName, 
			_activeTagsDict.Values);
		
		Parse();
	}

	private void Parse()
	{
		try
		{
			var parseResult = TaggedNameParser.ParseTaggedName(SourceName);

			if (!parseResult.HasTags)
			{
				Success = true;
				DoIgnore = false;
				DoRename = false;
				NewName = string.Empty;
				
				Log.Debug("There were no tags in the name {Name}", SourceName);
				
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
					SourceName, 
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
					SourceName,
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
					SourceName,
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
		    throw new NameParserException($"Found the unrecognized tag {tag} in the name {SourceName}");

	    return _activeTagsDict.ContainsKey(tagname);
    }

    private readonly Configuration _configuration;
    private readonly IReadOnlyDictionary<string, string> _activeTagsDict;
    private readonly Dictionary<string, string> _additionalTagsFoundInName = new(StringComparer.InvariantCultureIgnoreCase);
}