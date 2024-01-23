using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;
using vigobase;

namespace vigoscope;

internal static partial class TaggedNameParser
{
    // ReSharper disable once MemberCanBePrivate.Global

    internal static TaggedNameParseResult ParseTaggedName(string originalName)
    {
        Log.Debug("Parsing the name \"{Name}\"", originalName);
	    
        var noTagsFoundResult = new TaggedNameParseResult(
            originalName, 
            false, 
            false,
            originalName, 
            false, 
            Array.Empty<NamedTag>(), 
            Array.Empty<NamedTag>(), 
            Array.Empty<NamedTag>());

        if (string.IsNullOrWhiteSpace(originalName))
        {
            Log.Warning("{ClassName}.{MethodName} was called with an empty name",
                nameof(NameParseResult),
                nameof(ParseTaggedName));

            return noTagsFoundResult;
        }

        var matches = RexTaggedRegion.Matches(originalName);

        if (matches.Count == 0)
        {
            Log.Debug("Name does not match the tag pattern");
		    
            return noTagsFoundResult;
        }

        if (matches.Count != 1)
        {
            Log.Warning("{ClassName}.{MethodName} Found {MatchCount} tag matches when only a single match is allowed",
                nameof(NameParseResult),
                nameof(ParseTaggedName),
                matches.Count);
		    
            return noTagsFoundResult;
        }

        if (matches[0] is not { } match ||
            !match.Groups["sepc"].Success ||
            match.Groups["sepc"].Value.Length != 1 ||
            !match.Groups["words"].Success
           )
        {
            Log.Warning("{ClassName}.{MethodName} Parsing the name seemed to be successful, but the result violates basic assumptions",
                nameof(NameParseResult),
                nameof(ParseTaggedName));
		    
            return noTagsFoundResult;
        }

        var separatorChar = match.Groups["sepc"].Value[0];
        var chainedTags = match.Groups["words"].Value;

        // Log.Debug("{Parameter} = {Value}", nameof(separatorChar), separatorChar);
        Log.Debug("{Parameter} = {Value}", nameof(chainedTags), chainedTags);
	    
        var tagPhraseSegments = chainedTags.Split(separatorChar)
            .Select(content => new TagPhraseSegment(content))
            .ToList();
	    
        var sbSyntaxString = new StringBuilder();
        foreach (var tagPhraseSegment in tagPhraseSegments)
        {
            sbSyntaxString.Append(tagPhraseSegment.SyntaxChar);
        }

        if (!RexSyntaxRules.IsMatch(sbSyntaxString.ToString()))
        {
            if (3 < tagPhraseSegments.Count ||
                (1 < tagPhraseSegments.Count &&
                 tagPhraseSegments.Select(e => e.Content)
                     .Any(s => s.Equals("skip", StringComparison.InvariantCultureIgnoreCase) ||
                               s.Equals("deploy", StringComparison.InvariantCultureIgnoreCase) ||
                               s.Equals("only", StringComparison.InvariantCultureIgnoreCase) ||
                               s.Equals("except", StringComparison.InvariantCultureIgnoreCase) ||
                               s.Equals("tags", StringComparison.InvariantCultureIgnoreCase) ||
                               s.Equals("tag", StringComparison.InvariantCultureIgnoreCase))))
            {
                Log.Error("{ClassName}.{MethodName} Candidate tag list violates syntax and was rejected. It resembles so closely valid syntax, that this is considered an error. Name = {Name}, Separator = {Separator}, TagChain = {TagChain}, SyntaxString = {SyntaxString}",
                    nameof(NameParseResult),
                    nameof(ParseTaggedName),			    
                    originalName,
                    separatorChar,
                    chainedTags,
                    sbSyntaxString.ToString());
                                
                throw new NameParserException($"The name \"{originalName}\" has no recognizable tags, but it matches so closely that this is considered an error");
            }

            Log.Warning("{ClassName}.{MethodName} Candidate tag list violates syntax and was rejected. Name = {Name}, Separator = {Separator}, TagChain = {TagChain}, SyntaxString = {SyntaxString}",
                nameof(NameParseResult),
                nameof(ParseTaggedName),			    
                originalName,
                separatorChar,
                chainedTags,
                sbSyntaxString.ToString());
            
            return noTagsFoundResult;
        }
	    
        if (tagPhraseSegments.Any(e => !e.IsValid))
        {
            Log.Warning("{ClassName}.{MethodName} Found invalid tags {TagList}",
                nameof(NameParseResult),
                nameof(ParseTaggedName),			    
                tagPhraseSegments.Where(e => !e.IsValid).Select(s => s.Content).ToList());
		    
            return noTagsFoundResult;
        }

        var primaryTags = new List<string>();
        var secondaryTags = new List<string>();
        var additionalTags = new List<string>();

        var currentList = new List<string>();
	    
        foreach (var tagPhraseSegment in tagPhraseSegments)
        {
            if (tagPhraseSegment.IsKeyword)
                currentList = tagPhraseSegment.SyntaxChar switch
                {
                    'S' or 'O' => primaryTags,
                    'E' => secondaryTags,
                    'T' => additionalTags,
                    _ => throw new NameParserException($"Unmatched SyntaxChar {tagPhraseSegment.SyntaxChar} while collection tags")
                };
            else if (tagPhraseSegment.IsTag)
                currentList.Add(tagPhraseSegment.Content);
        }

        Log.Debug("{Parameter} = {Value}", nameof(primaryTags), primaryTags);
        Log.Debug("{Parameter} = {Value}", nameof(secondaryTags), secondaryTags);
        Log.Debug("{Parameter} = {Value}", nameof(additionalTags), additionalTags);

        var isInclusive = tagPhraseSegments.Any(e => e.SyntaxChar == 'O');
	    
        Log.Debug("{Parameter} = {Value}", nameof(isInclusive), isInclusive);

        if (primaryTags.Count == 0 && additionalTags.Count == 0 ||
            primaryTags.Count == 0 && secondaryTags.Count > 0)
        {
            Log.Warning("{ClassName}.{MethodName} Primary tag list is empty and either additional tag list is empty or secondary tag list is not empty",
                nameof(NameParseResult),
                nameof(ParseTaggedName));
		    
            return noTagsFoundResult;
        }
	    
        var cleanName = (match.Index < 1 ? string.Empty : originalName[..match.Index]) +
                        (match.Index + match.Length < originalName.Length ? originalName[(match.Index + match.Length)..] : string.Empty);

        Log.Debug("{Parameter} = {Value}", nameof(cleanName), cleanName);

        var parseResult = new TaggedNameParseResult(
            originalName, 
            true,
            primaryTags.Count > 0,
            cleanName, 
            isInclusive, 
            primaryTags.Select(s => new NamedTag(s)).ToImmutableList(),
            secondaryTags.Select(s => new NamedTag(s)).ToImmutableList(),
            additionalTags.Select(s => new NamedTag(s)).ToImmutableList());
	    
        return parseResult;
    }
    
    public static char[] Delimiters { get; } = DelimChars.ToCharArray();

    private const string DelimChars = "~";

    private static readonly Regex RexTaggedRegion = RexTaggedRegionPartial();
    private static readonly Regex RexSyntaxRules = RexSyntaxRulesPartial();

    [GeneratedRegex("(?'sepc'[" + DelimChars + "])(?'words'[a-zA-Z][-_.a-zA-Z0-9]{2,39}([" + DelimChars + @"][a-zA-Z][-_.a-zA-Z0-9]{2,39}){1,80})\k<sepc>\k<sepc>", RegexOptions.ExplicitCapture)]
    private static partial Regex RexTaggedRegionPartial();
    
    [GeneratedRegex("^((Tx{1,100})|((SD|DO)x{1,100}(Ex{1,100})?(Tx{1,100})?)|(Tx{1,100}(SD|DO)x{1,100}(Ex{1,100})?))$", RegexOptions.ExplicitCapture)]
    private static partial Regex RexSyntaxRulesPartial();
}