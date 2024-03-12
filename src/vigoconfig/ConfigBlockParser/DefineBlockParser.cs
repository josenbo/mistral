using System.Text.RegularExpressions;
using Serilog;
using vigobase;

namespace vigoconfig;

internal partial class DefineBlockParser(CommonDefinitions commonDefinitions, SourceBlock codeBlock)
{
    public void Parse()
    {
        if (codeBlock.Lines.Count < 2)
            throw new VigoFatalException(AppEnv.Faults.Fatal(
                "FX238",
                $"The {nameof(ConfigurationReader)} should guarantee that there is at least the header line and the closing DONE",
                string.Empty));

        var match = RexHeader.Match(codeBlock.Lines[0].Content);

        if (!match.Success)
            throw new VigoFatalException(AppEnv.Faults.Fatal(
                "FX245",
                null,
                $"Expecting a file list definition DEFINE FILE LIST <name> but found in line #{codeBlock.Lines[0].LineNumber} of {codeBlock.ConfigurationFile}: {codeBlock.Lines[0].Content}"));

        if (!RexEndBlock.IsMatch(codeBlock.Lines[^1].Content))
            throw new VigoFatalException(AppEnv.Faults.Fatal(
                "FX595",
                $"The {nameof(ConfigurationReader)} should guarantee that there is a closing DONE",
                string.Empty));

        var listName = match.Groups["listName"].Value.Trim();

        var entries = new Dictionary<string, FileListEntry>(StringComparer.Ordinal); 
            
        for (var i = 1; i < codeBlock.Lines.Count-1; i++)
        {
            var line = codeBlock.Lines[i].Content;

            if (RexEmptyOrComment.IsMatch(line))
                continue;

            line = line.Trim();

            var entry = new FileListEntry(line, null);
            if (!entries.TryAdd(entry.FilenameToMatch, entry))
                Log.Warning("Duplicate file name in {TheFile} will be ignored. {TheLine}",
                    codeBlock.ConfigurationFile,
                    codeBlock.Lines[i]);
        }

        var namedFileList = new NamedFileList(listName, entries);

        if (!commonDefinitions.NamedFileListDict.TryAdd(namedFileList.ListName, namedFileList))
            throw new VigoFatalException(AppEnv.Faults.Fatal(
                "FX602",
                null,
                $"Duplicate file list name {listName} in {codeBlock.ConfigurationFile} at line #{codeBlock.FromLineNumber}"));
    }

    private static readonly Regex RexHeader = RexHeaderCompiled();
    private static readonly Regex RexEndBlock = RexEndBlockCompiled();
    private static readonly Regex RexEmptyOrComment = RexEmptyOrCommentCompiled();

    #region Generated Embedded Regex

    [GeneratedRegex(@"^\s*DEFINE\s+FILE\s+LIST\s+(?'listName'[a-zA-Z][-_.a-zA-Z0-9]{0,79})\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant)]
    private static partial Regex RexHeaderCompiled();
    
    [GeneratedRegex(@"^\s*done\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RexEndBlockCompiled();
    
    [GeneratedRegex(@"^((\s*)|(\s*#.*))$")]
    private static partial Regex RexEmptyOrCommentCompiled();

    #endregion
}