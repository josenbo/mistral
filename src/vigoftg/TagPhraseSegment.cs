using System.Text.RegularExpressions;

namespace vigoftg;

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
		
        if (content.Equals("SKIP", StringComparison.Ordinal))
        {
            IsTag = false;
            IsKeyword = true;
            SyntaxChar = 'S';
            IsValid = true;
        }
        else if (content.Equals("DEPLOY", StringComparison.Ordinal))
        {
            IsTag = false;
            IsKeyword = false;
            SyntaxChar = 'D';
            IsValid = true;
        }
        else if (content.Equals("ONLY", StringComparison.Ordinal))
        {
            IsTag = false;
            IsKeyword = true;
            SyntaxChar = 'O';
            IsValid = true;
        }
        else if (content.Equals("EXCEPT", StringComparison.Ordinal))
        {
            IsTag = false;
            IsKeyword = true;
            SyntaxChar = 'E';
            IsValid = true;
        }
        else if (content.Equals("TAGS", StringComparison.Ordinal))
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
            IsValid = RexTagName.IsMatch(content);
        }
    }
	
    internal static readonly Regex RexTagName = new Regex(@"^[a-zA-Z][a-zA-Z0-9]{0,50}([-_][a-zA-Z0-9]{1,50}){0,100}$", RegexOptions.ExplicitCapture);
}