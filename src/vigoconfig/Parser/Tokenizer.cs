using System.Text;
using Serilog;

namespace vigoconfig;

internal class Tokenizer(SourceBlock sourceBlock)
{
    public bool AtEnd => _content.Length <= _contentPosition;

    public bool Peek(params string[] compareWith)
    {
        var startpos = _contentPosition;
        
        foreach (var expected in compareWith)
        {
            if (AtEnd)
                return false;
        
            var token = GetNextToken();

            if (expected.Equals(token, StringComparison.InvariantCultureIgnoreCase)) 
                continue;
            
            _contentPosition = startpos;
            return false;
        }

        return true;
    }
    
    public bool Check(params string[] compareWith)
    {
        var sb = new StringBuilder();
        var startpos = _contentPosition;
        
        foreach (var expected in compareWith)
        {
            if (AtEnd)
                return false;
        
            var token = GetNextToken();
            sb.Append(token);
            
            if (!expected.Equals(token, StringComparison.InvariantCultureIgnoreCase))
            {
                Log.Error("Expected the token sequence {ExpectedSequence} at line {TheLine} but found {ObservedSequence}",
                    compareWith,
                    GetLineNumberFromPosition(startpos),
                    sb.ToString());
                return false;
            }

            sb.Append(' ');
        }

        return true;
    }
    
    public string GetNextToken()
    {
        var sb = new StringBuilder();
        var trim = false;

        while (!AtEnd && char.IsWhiteSpace(_content[_contentPosition]))
            _contentPosition++;

        if (!AtEnd && _content[_contentPosition] == '=')
        {
            _contentPosition++;
            trim = true;
			
            while (!AtEnd && _content[_contentPosition] is not '\r' or '\n')
            {
                sb.Append(_content[_contentPosition]);
                _contentPosition++;
            }
        }
        else
        {
            while (!AtEnd && !char.IsWhiteSpace(_content[_contentPosition]))
            {
                sb.Append(_content[_contentPosition]);
                _contentPosition++;
            }
        }
        
        while (!AtEnd && char.IsWhiteSpace(_content[_contentPosition]))
            _contentPosition++;
        
        return trim ? sb.ToString().Trim( ): sb.ToString();
    }

    public string ReadToEnoOfLineAndTrim()
    {
        var sb = new StringBuilder();

        while (!AtEnd && _content[_contentPosition] is not '\r' or '\n')
        {
            sb.Append(_content[_contentPosition]);
            _contentPosition++;
        }

        while (!AtEnd && char.IsWhiteSpace(_content[_contentPosition]))
            _contentPosition++;

        return sb.ToString().Trim();
    }

    public SourceLine GetCurrentSourceLine()
    {
        var position = _contentPosition;
        
        foreach (var sourceLine in sourceBlock.Lines)
        {
            if (position < sourceLine.Content.Length)
                return sourceLine;
            position -= sourceLine.Content.Length;
        }
        return sourceBlock.Lines[^1];
    }
    
    private int GetLineNumberFromPosition(int position)
    {
        foreach (var sourceLine in sourceBlock.Lines)
        {
            if (position < sourceLine.Content.Length)
                return sourceLine.LineNumber;
            position -= sourceLine.Content.Length;
        }
        return sourceBlock.LastLineNumber;
    }

    private readonly string _content = sourceBlock.Content;
    private int _contentPosition = 0;
}