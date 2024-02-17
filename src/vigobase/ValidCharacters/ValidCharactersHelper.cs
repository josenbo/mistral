using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Serilog;

namespace vigobase;

[SuppressMessage("Performance", "SYSLIB1045:Convert to \'GeneratedRegexAttribute\'.")]
public static partial class ValidCharactersHelper
{
    public static Regex? ParseConfiguration(string configText)
    {
        if (string.IsNullOrWhiteSpace(configText))
        {
            Log.Fatal("The allowed characters setting can be omitted, but when used, the value must not be empty");
            throw new VigoFatalException("Invalid empty value for the allowed characters setting");
        }
        
        if (string.IsNullOrWhiteSpace(configText) || configText.Trim().Equals("all", StringComparison.InvariantCultureIgnoreCase))
            return null;

        var match = RexConfigLine.Match(configText);
        
        if (!match.Success)
        {
            Log.Fatal("The allowed characters value could not be parsed. Expecting All|Ascii( + others)|AsciiGerman( + others). Value was: {TheValue}", configText);
            throw new VigoFatalException("Invalid value for the allowed characters setting");
        }
        
        var key = match.Groups["key"].Value.ToLowerInvariant();
        var other = match.Groups["other"].Success ? match.Groups["other"].Value.Trim() : string.Empty;
        
        switch (key)
        {
            // ReSharper disable StringLiteralTypo
            case "ascii" when string.IsNullOrWhiteSpace(other):
                return new Regex($"^[{AsciiChars}]*$", RegexOptions.Singleline);
            case "ascii":
                return new Regex($"^[{AsciiChars}{other}]*$", RegexOptions.Singleline);
            case "asciigerman" when string.IsNullOrWhiteSpace(other):
                return new Regex($"^[{AsciiGermanChars}]*$", RegexOptions.Singleline);
            case "asciigerman":
                return new Regex($"^[{AsciiGermanChars}{other}]*$", RegexOptions.Singleline);
            // ReSharper restore StringLiteralTypo
            default:
                Log.Fatal("Invalid key value for the allowed characters setting. Value was: {TheValue}", key);
                throw new VigoFatalException("Invalid key value for the allowed characters setting");
        }
    }

    private const string AsciiChars = @"\u0000-\u007F";
    private const string AsciiGermanChars = AsciiChars + "äöüÄÖÜß€";
    private static readonly Regex RexConfigLine = CompiledRexConfigLine();

    // ReSharper disable StringLiteralTypo
    [GeneratedRegex(@"^\s*\b(?'key'ascii|asciigerman)\b(\s*\+\s*(?'other'\S+))?\s*$", 
        RegexOptions.IgnoreCase | 
        RegexOptions.ExplicitCapture | 
        RegexOptions.Singleline | 
        RegexOptions.CultureInvariant)]
    // ReSharper restore StringLiteralTypo
    private static partial Regex CompiledRexConfigLine();
}