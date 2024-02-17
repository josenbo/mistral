using System.Text;
using Serilog;
using vigobase;

namespace vigoconfig;

internal class FolderBlockParser(FolderConfig folderConfig, SourceBlockFolder folderBlock)
{
    public void Parse()
    {
        if (!ParseValues())
            throw new VigoParseFolderConfigException(
                "Encountered syntax errors or illegal values in the folder configuration");
    }

    private bool ParseValues()
    {
        if (!_tokenizer.Check("CONFIGURE", "FOLDER"))
            return false;

        while (!_tokenizer.AtEnd)
        {
            if (_tokenizer.Peek("DONE"))
                return true;

            if (_tokenizer.Peek("DEFAULT", "FOR", "FILE", "MODE"))
            {
                var value = _tokenizer.GetNextToken().Trim();

                if (string.IsNullOrWhiteSpace(value))
                {
                    Log.Error("Missing value for default unix file mode", value);
                    return false;
                }
                
                if (FilePermission.TryParse(value, out var permission) && permission is FilePermissionOctal octalPermission)
                {
                    folderConfig.LocalDefaults ??= new FolderConfigPartialHandling();

                    folderConfig.LocalDefaults.StandardModeForFiles =
                        octalPermission.ComputeUnixFileMode(UnixFileMode.None);
                    continue;
                }
                else
                {
                    Log.Error("Could not derive a unix file mode from the value {TheFileModeValue}", value);
                    return false;
                }
            }

            if (_tokenizer.Peek("DEFAULT", "FOR", "SOURCE", "ENCODING"))
            {
                var value = _tokenizer.GetNextToken().Trim();
                
                if (string.IsNullOrWhiteSpace(value))
                {
                    Log.Error("Missing value for the default source encoding");
                    return false;
                }
                
                if (FileEncodingEnumHelper.TryParse(value, out var encoding))
                {
                    folderConfig.LocalDefaults ??= new FolderConfigPartialHandling();

                    folderConfig.LocalDefaults.SourceFileEncoding = encoding;
                    continue;
                }
                else
                {
                    Log.Error("Did not recognize the default source encoding {TheValue}", value);
                    return false;
                }
            }

            if (_tokenizer.Peek("DEFAULT", "FOR", "TARGET", "ENCODING"))
            {
                var value = _tokenizer.GetNextToken().Trim();
                
                if (string.IsNullOrWhiteSpace(value))
                {
                    Log.Error("Missing value for the default target encoding");
                    return false;
                }
                
                if (FileEncodingEnumHelper.TryParse(value, out var encoding))
                {
                    folderConfig.LocalDefaults ??= new FolderConfigPartialHandling();

                    folderConfig.LocalDefaults.TargetFileEncoding = encoding;
                    continue;
                }
                else
                {
                    Log.Error("Did not recognize the default target encoding {TheValue}", value);
                    return false;
                }
            }
            
            
            Log.Error("Could not parse the folder configuration line {TheLine}", _tokenizer.GetCurrentSourceLine());
            return false;
        }

        return false;
    }

    private readonly SourceBlockFolder _folderBlock = folderBlock;
    private readonly Tokenizer _tokenizer = new Tokenizer(folderBlock);
}

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