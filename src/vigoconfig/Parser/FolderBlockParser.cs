using Serilog;
using vigobase;

namespace vigoconfig;

internal class FolderBlockParser(FolderConfig folderConfig, SourceBlock codeBlock)
{
    #region local private class ConfigPhrase

    private class ConfigPhrase(bool required, Func<Tokenizer, FolderConfig, ConfigPhrase, bool> parseFunc, string name)
    {
        public string Name { get; } = name;
        public bool Required { get; } = required;
        public bool ParsedSuccessfully { get; set; }
        public bool PhraseFound { get; set; }
        public int PhraseOccurenceCount { get; set; }
        public int NumberOfSuccessfulMatches { get; set; }
        public Func<Tokenizer, FolderConfig, ConfigPhrase, bool> ParseFunc { get; set; } = parseFunc;
        public string? ErrorMessage { get; set; }
        public SourceLine? SourceLine { get; set; }

        public bool HasErrors()
        {
            if (NumberOfSuccessfulMatches == 1 && PhraseOccurenceCount == 1)
                return false;
            if (Required)
                return true;
            return 0 < PhraseOccurenceCount;
        }

        public bool ReturnPhraseNotFound()
        {
            ParsedSuccessfully = false;
            PhraseFound = false;
            return PhraseFound;
        }

        public bool ReturnParseWithErrors(string errorMessage, SourceLine? errorSourceLine = null)
        {
            ParsedSuccessfully = false;
            PhraseFound = true;
            PhraseOccurenceCount++;
            ErrorMessage = errorMessage;
            if (errorSourceLine is not null)
                SourceLine = errorSourceLine;
            return PhraseFound;
        }

        public bool ReturnSuccessfullyParsed()
        {
            ParsedSuccessfully = true;
            PhraseFound = true;
            PhraseOccurenceCount++;
            NumberOfSuccessfulMatches++;
            return PhraseFound;
        }
    }

    #endregion
    
    public void Parse()
    {
        if (!ParseValues())
        {
            Log.Fatal("Failed to parse the folder configuration {TheBlockDescription}." +
                      " There is a syntax error or an unrecognized value in the line {TheLine}." +
                      " The error message was {TheErrorMessage}",
                codeBlock.Description,
                _lastErrorSourceLine,
                _lastErrorMessage);
            
            throw new VigoParseFolderConfigException(
                "Failed to parse the folder configuration. Encountered syntax errors or illegal values in the folder configuration");
        }    
    }

    private bool ParseValues()
    {
        var phrases = new List<ConfigPhrase>()
        {
            new ConfigPhrase(false, ParseDefaultForFileMode, "Default File Mode"),
            new ConfigPhrase(false, ParseDefaultForSourceEncoding, "Default Source Encoding"),
            new ConfigPhrase(false, ParseDefaultForTargetEncoding, "Default Target Encoding")
        };

        if (!_tokenizer.Check("CONFIGURE", "FOLDER"))
        {
            _lastErrorSourceLine = _tokenizer.GetCurrentSourceLine();
            _lastErrorMessage = "The folder defaults and settings section must start with CONFIGURE FOLDER";
            return false;
        }

        while (!_tokenizer.AtEnd)
        {
            if (_tokenizer.Peek("DONE"))
            {
                var retval = true;
                var isFirstError = true;
                
                foreach (var phrase in phrases.Where(p => p.HasErrors()))
                {
                    if (isFirstError)
                    {
                        Log.Debug("Showing errors for {TheBlockDescription}", codeBlock.Description);
                        retval = false;
                        isFirstError = false;
                    }

                    _lastErrorSourceLine = phrase.SourceLine;
                    _lastErrorMessage = phrase.ErrorMessage;
                    
                    Log.Debug("{TheErrorMessage} on line {TheLine}", phrase.ErrorMessage, phrase.SourceLine);
                }
                
                return retval;
            }
            
            // Match the first phrase that has not already been matched
            
            var initialMatch = phrases
                .Where(e => !e.PhraseFound)
                .FirstOrDefault(p => p.ParseFunc(_tokenizer, folderConfig, p));

            if (initialMatch is not null)
            {
                if (!initialMatch.HasErrors() && initialMatch.ParsedSuccessfully)
                    continue;
                
                _lastErrorSourceLine = initialMatch.SourceLine;
                _lastErrorMessage = initialMatch.ErrorMessage;
                    
                Log.Debug("Parsing failed with the message {TheErrorMessage} on line {TheLine}", initialMatch.ErrorMessage, initialMatch.SourceLine);
                
                return false;
            }
            
            // We are definitely in an error state. To give mor information to the user
            // we check, if an already parsed line appears a second time

            _lastErrorSourceLine = _tokenizer.GetCurrentSourceLine();
            
            var duplicateMatch = phrases
                .Where(e => e.PhraseFound)
                .FirstOrDefault(p => p.ParseFunc(_tokenizer, folderConfig, p));

            if (duplicateMatch is not null)
            {
                _lastErrorMessage = $"Parsing failed because of a duplicate entry for {duplicateMatch.Name} on line {_lastErrorSourceLine.LineNumber}";
                    
                Log.Debug("Parsing failed because of a duplicate entry for {ThePhrase} on line {TheLine}", duplicateMatch.Name, _lastErrorSourceLine);
                
                return false;
            }

            _lastErrorMessage = $"There is unrecognized content on line {_lastErrorSourceLine.LineNumber}";
            
            Log.Debug("There is unrecognized content on line {TheLine}", _lastErrorSourceLine);
            
            return false;
        }

        _lastErrorSourceLine = codeBlock.Lines[^1];
        _lastErrorMessage = "The folder defaults and settings section must be closet with DONE";
        return false;
    }

    private static bool ParseDefaultForFileMode(Tokenizer tokenizer, FolderConfig folder, ConfigPhrase phrase)
    {
        if (!tokenizer.Peek("DEFAULT", "FOR", "FILE", "MODE"))
            return phrase.ReturnPhraseNotFound();

        phrase.SourceLine = tokenizer.GetCurrentSourceLine();

        if (tokenizer.AtEnd)
        {
            const string message = "Encountered end of source when looking for the default unix file mode"; 
            Log.Debug(message);
            return phrase.ReturnParseWithErrors(message);
        }
        var value = tokenizer.GetNextToken();

        if (string.IsNullOrWhiteSpace(value))
        {
            const string message = "Missing value for default unix file mode"; 
            Log.Debug(message);
            return phrase.ReturnParseWithErrors(message);
        }

        if (!FilePermission.TryParse(value, out var permission) ||
            permission is not FilePermissionOctal octalPermission)
        {
            Log.Debug("Could not derive a unix file mode from the value {TheFileModeValue}. Expecting three octal digits like 644", value);
            return phrase.ReturnParseWithErrors($"Could not derive a unix file mode from the value {value}. Expecting three octal digits like 644");
        }

        folder.LocalDefaults ??= new FolderConfigPartialHandling();

        folder.LocalDefaults.StandardModeForFiles =
            octalPermission.ComputeUnixFileMode(UnixFileMode.None);

        return phrase.ReturnSuccessfullyParsed();
    }

    private static bool ParseDefaultForSourceEncoding(Tokenizer tokenizer, FolderConfig folder, ConfigPhrase phrase)
    {
        if (!tokenizer.Peek("DEFAULT", "FOR", "SOURCE", "ENCODING"))
            return phrase.ReturnPhraseNotFound();

        phrase.SourceLine = tokenizer.GetCurrentSourceLine();

        if (tokenizer.AtEnd)
        {
            const string message = "Encountered end of source when looking for the default source encoding"; 
            Log.Debug(message);
            return phrase.ReturnParseWithErrors(message);
        }
        var value = tokenizer.GetNextToken();

        if (string.IsNullOrWhiteSpace(value))
        {
            const string message = "Missing value for default source encoding"; 
            Log.Debug(message);
            return phrase.ReturnParseWithErrors(message);
        }

        if (!FileEncodingEnumHelper.TryParse(value, out var encoding))
        {
            Log.Debug("Could not recognize a source encoding with the name {TheEncodingValue}. Valid names are: {ValidNames}", 
                value,
                FileEncodingEnumHelper.ValidNames);
            return phrase.ReturnParseWithErrors($"Could not recognize a source encoding with the name {value}. Valid names are: {string.Join(", ", FileEncodingEnumHelper.ValidNames)}");
        }

        folder.LocalDefaults ??= new FolderConfigPartialHandling();

        folder.LocalDefaults.SourceFileEncoding = encoding;

        return phrase.ReturnSuccessfullyParsed();
    }
    
    private static bool ParseDefaultForTargetEncoding(Tokenizer tokenizer, FolderConfig folder, ConfigPhrase phrase)
    {
        if (!tokenizer.Peek("DEFAULT", "FOR", "TARGET", "ENCODING"))
            return phrase.ReturnPhraseNotFound();

        phrase.SourceLine = tokenizer.GetCurrentSourceLine();

        if (tokenizer.AtEnd)
        {
            const string message = "Encountered end of source when looking for the default target encoding"; 
            Log.Debug(message);
            return phrase.ReturnParseWithErrors(message);
        }
        var value = tokenizer.GetNextToken();

        if (string.IsNullOrWhiteSpace(value))
        {
            const string message = "Missing value for default target encoding"; 
            Log.Debug(message);
            return phrase.ReturnParseWithErrors(message);
        }

        if (!FileEncodingEnumHelper.TryParse(value, out var encoding))
        {
            Log.Debug("Could not recognize a target encoding with the name {TheEncodingValue}. Valid names are: {ValidNames}", 
                value,
                FileEncodingEnumHelper.ValidNames);
            return phrase.ReturnParseWithErrors($"Could not recognize a target encoding with the name {value}. Valid names are: {string.Join(", ", FileEncodingEnumHelper.ValidNames)}");
        }

        folder.LocalDefaults ??= new FolderConfigPartialHandling();

        folder.LocalDefaults.SourceFileEncoding = encoding;

        return phrase.ReturnSuccessfullyParsed();
    }

    private readonly Tokenizer _tokenizer = new Tokenizer(codeBlock);
    private SourceLine? _lastErrorSourceLine;
    private string? _lastErrorMessage;
}
