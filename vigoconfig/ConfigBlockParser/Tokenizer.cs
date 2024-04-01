using System.Text;
using Serilog;
using vigobase;

namespace vigoconfig;

internal class Tokenizer(SourceBlock sourceBlock)
{
    public bool AtEnd => _content.Length <= _contentPosition;

    public IReadOnlyList<string> MatchedTokens => _matchedTokens;

    // public bool Peek(params string[] compareWith)
    // {
    //     var startPosition = _contentPosition;
    //     
    //     foreach (var expected in compareWith)
    //     {
    //         if (AtEnd)
    //             return false;
    //     
    //         var token = GetNextToken();
    //
    //         if (expected.Equals(token, StringComparison.InvariantCultureIgnoreCase)) 
    //             continue;
    //         
    //         _contentPosition = startPosition;
    //         return false;
    //     }
    //
    //     return true;
    // }
    
    public bool TryReadTokens(params string[][] compareWith)
    {
        var startPosition = _contentPosition;
        
        _matchedTokens.Clear();
        
        foreach (var expectedArray in compareWith)
        {
            if (AtEnd)
                return false;

            var currentTokenStart = _contentPosition;
            var isOptionalToken = false;

            var found = false;

            if (expectedArray is ["*"])
            {   
                var token = ReadToEnoOfLineAndTrim();

                _matchedTokens.Add(token);
                found = true;
            }
            else
            {
                var token = GetNextToken();

                foreach (var expected in expectedArray)
                {
                    if (string.IsNullOrWhiteSpace(expected))
                    {
                        isOptionalToken = true;
                        continue;
                    }

                    if (!expected.Equals(token, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    _matchedTokens.Add(expected);
                    found = true;
                    break;
                }
            }

            if (found) 
                continue;

            if (isOptionalToken)
            {
                _matchedTokens.Add(string.Empty);
                _contentPosition = currentTokenStart;
                continue;
            }
            
            _matchedTokens.Clear();
            _contentPosition = startPosition;
            return false;
        }
        
        Log.Debug("{TheTokenizer} matched the tokens {MatchedTokens} from the syntax definition {TheSyntaxDefinition}", 
            nameof(Tokenizer), 
            _matchedTokens, 
            compareWith);

        if (_matchedTokens.Count == compareWith.Length) 
            return true;
        
        Log.Fatal("The matched tokens must be the same number as in the syntax definition, but there were {TheObservedNumber} instead of the expected {TheExpectedNumber}",
            _matchedTokens.Count,
            compareWith.Length);

        throw new VigoFatalException(AppEnv.Faults.Fatal(
            "FX210",
            null,
            "Failed to parse a folder configuration script. See log for details"));
    }
    
    // public bool Check(params string[] compareWith)
    // {
    //     var sb = new StringBuilder();
    //     var startPosition = _contentPosition;
    //     
    //     foreach (var expected in compareWith)
    //     {
    //         if (AtEnd)
    //             return false;
    //     
    //         var token = GetNextToken();
    //         sb.Append(token);
    //         
    //         
    //         if (!expected.Equals(token, StringComparison.InvariantCultureIgnoreCase))
    //         {
    //             Log.Error("Expected the token sequence {ExpectedSequence} at line {TheLine} but found {ObservedSequence}",
    //                 compareWith,
    //                 GetLineNumberFromPosition(startPosition),
    //                 sb.ToString());
    //             return false;
    //         }
    //
    //         sb.Append(' ');
    //     }
    //
    //     return true;
    // }

    private string GetNextToken()
    {
        var sb = new StringBuilder();
        var trim = false;

        while (!AtEnd && char.IsWhiteSpace(_content[_contentPosition]))
            _contentPosition++;

        if (!AtEnd && _content[_contentPosition] == '=')
        {
            _contentPosition++;
            trim = true;
			
            while (!AtEnd && _content[_contentPosition] != '\r' && _content[_contentPosition] != '\n')
            {
                sb.Append(_content[_contentPosition]);
                _contentPosition++;
            }
        }
        else
        {
            while (!AtEnd && char.IsLetterOrDigit(_content[_contentPosition]))
            {
                sb.Append(_content[_contentPosition]);
                _contentPosition++;
            }
        }
        
        while (!AtEnd && char.IsWhiteSpace(_content[_contentPosition]))
            _contentPosition++;
        
        return trim ? sb.ToString().Trim( ): sb.ToString();
    }

    private string ReadToEnoOfLineAndTrim()
    {
        var sb = new StringBuilder();

        while (!AtEnd && _content[_contentPosition] != '\r' && _content[_contentPosition] != '\n')
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
        return sourceBlock.ToLineNumber;
    }
    
    private readonly string _content = sourceBlock.Content;
    private readonly List<string> _matchedTokens = [];
    private int _contentPosition;
}