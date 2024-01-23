using vigobase;

namespace vigoscope;

internal class TagPhraseSegment
{
    internal bool IsTag { get; }
    internal bool IsKeyword { get; }
    internal char SyntaxChar { get; }
    internal string Content { get; }
    internal bool IsValid { get; }

    internal TagPhraseSegment(string content)
    {
        Content = content;
		
        if (content.Equals("SKIP", StringComparison.InvariantCultureIgnoreCase))
        {
            IsTag = false;
            IsKeyword = true;
            SyntaxChar = 'S';
            IsValid = true;
        }
        else if (content.Equals("DEPLOY", StringComparison.InvariantCultureIgnoreCase))
        {
            IsTag = false;
            IsKeyword = false;
            SyntaxChar = 'D';
            IsValid = true;
        }
        else if (content.Equals("ONLY", StringComparison.InvariantCultureIgnoreCase))
        {
            IsTag = false;
            IsKeyword = true;
            SyntaxChar = 'O';
            IsValid = true;
        }
        else if (content.Equals("EXCEPT", StringComparison.InvariantCultureIgnoreCase))
        {
            IsTag = false;
            IsKeyword = true;
            SyntaxChar = 'E';
            IsValid = true;
        }
        else if (content.Equals("TAGS", StringComparison.InvariantCultureIgnoreCase))
        {
            IsTag = false;
            IsKeyword = true;
            SyntaxChar = 'T';
            IsValid = true;
        }
        else 
        {
            IsTag = true;
            IsKeyword = false;
            SyntaxChar = 'x';
            try
            {
                var namedTag = new NamedTag(content);
                IsValid = true;
            }
            catch (ArgumentException)
            {
                IsValid = false;
            }
        }
    }
}