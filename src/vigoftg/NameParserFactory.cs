using JetBrains.Annotations;

namespace vigoftg;

[PublicAPI]
public class NameParserFactory
{
    #region public static members

    public static INameParser Create(params string[] tags) => AddTagsAndBuild(tags);

    public static INameParser Create(IEnumerable<string> tags) => AddTagsAndBuild(tags);

    #endregion
    
    #region public instance members

    public NameParserFactory()
    {
    }

    public NameParserFactory(params string[] tags)
    {
        AddTagsImplementation(tags);    
    }
    
    public NameParserFactory(IEnumerable<string> tags)
    {
        AddTagsImplementation(tags);
    }

    public NameParserFactory AddTag(string tag) => AddTagImplementation(tag);

    public NameParserFactory AddTags(params string[] tags) => AddTagsImplementation(tags);

    public NameParserFactory AddTags(IEnumerable<string> tags) => AddTagsImplementation(tags);
    
    public INameParser Build()
    {
        return new NameParser(_configuration);
    }
    
    #endregion

    #region internals

    #region private static members

    private static INameParser AddTagsAndBuild(IEnumerable<string> tags)
    {
        return new NameParserFactory()
            .AddTags(tags)
            .Build();
    }

    #endregion

    #region private instance members

    private NameParserFactory AddTagsImplementation(IEnumerable<string> tags)
    {
        foreach (var tag in tags) 
        {
            AddCaseInsensitiveTag(tag);
        }
        return this;
    }

    private NameParserFactory AddTagImplementation(string tag)
    {
        AddCaseInsensitiveTag(tag);
        return this;	
    }
    private void AddCaseInsensitiveTag(string tag)
    {
        if (!_configuration.AddTag(isCaseSensitive: false, tag))
        {
            throw new NameParserException($"Failed to add the tag {tag} for file name parsing");
        }
    }

    private readonly Configuration _configuration = new();

    #endregion

    #endregion
}