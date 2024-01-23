using Serilog;
using vigobase;

namespace vigoftg;

internal class NameParseResult : INameParseResult
{
	public bool CanDeploy { get; private set; } = true;
	public string SourceName { get; }
	public string TargetName { get; private set; }
	public IEnumerable<NamedTag> Tags => _additionalTagsFoundInName;

	internal NameParseResult(IEnvironmentDescriptor descriptor, string name)
	{
		_descriptor = descriptor;
		TargetName = SourceName = name;

		// Log.Debug("Parsing the name {Name}", SourceName);
		
		Parse();
	}

	private void Parse()
	{
		var parseResult = TaggedNameParser.ParseTaggedName(SourceName);

		if (!parseResult.HasTags)
		{
			Log.Debug("There were no tags in the name {Name}", SourceName);
			return;
		}

		_additionalTagsFoundInName.AddRange(parseResult.AdditionalTags);
		
		if (!parseResult.HasScope)
		{
			TargetName = parseResult.NewName;
			
			Log.Debug("There were only additional tags in the name {Name}, but no scope tags. Additional tags = {AdditionalTags}", 
				SourceName, 
				_additionalTagsFoundInName.Select(s => s.Name));
			return;
		}
	
		var inScope = parseResult.IsInclusive
			? _descriptor.HasAnyOfTheseTags(parseResult.PrimaryTags)
			: _descriptor.HasNoneOfTheseTags(parseResult.PrimaryTags);
    
		if (parseResult.SecondaryTags.Any())
			inScope = parseResult.IsInclusive
				? _descriptor.HasNoneOfTheseTags(parseResult.SecondaryTags)
				: _descriptor.HasAnyOfTheseTags(parseResult.SecondaryTags);

		if (inScope)
		{
			TargetName = parseResult.NewName;
			
			Log.Debug("The name {Name} is tagged and will be renamed to {NewName}. The {TagMeaning} tags where {TagsList} with the exception of {ExceptedTagsList}. This matched the environment descriptor {EnvironmentDescriptor}",
				SourceName,
				TargetName,
				parseResult.IsInclusive ? "inclusive" : "exclusive",
				parseResult.PrimaryTags,
				parseResult.SecondaryTags,
				_descriptor
			);
		}
		else
		{
			CanDeploy = false;
			TargetName = string.Empty;
			_additionalTagsFoundInName.Clear();
			
			Log.Debug("The name {Name} is tagged but out of scope. The {TagMeaning} tags where {TagsList} with the exception of {ExceptedTagsList}. This did not match the environment descriptor {EnvironmentDescriptor}",
				SourceName,
				parseResult.IsInclusive ? "inclusive" : "exclusive",
				parseResult.PrimaryTags,
				parseResult.SecondaryTags,
				_descriptor
			);
		}
	}

	private readonly IEnvironmentDescriptor _descriptor;
	private readonly List<NamedTag> _additionalTagsFoundInName = [];
}