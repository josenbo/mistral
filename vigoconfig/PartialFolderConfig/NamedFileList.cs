using System.Diagnostics.CodeAnalysis;

namespace vigoconfig;

internal record NamedFileList(string ListName, IReadOnlyDictionary<string, FileListEntry> FileNameDict) : INameTestAndReplaceHandler
{
    public string Identification => ListName;

    public bool TestName(string theNameToTest, [NotNullWhen(true)] out string? theNewName)
    {
        if (FileNameDict.TryGetValue(theNameToTest, out var theEntry))
        {
            theNewName = theEntry.ReplaceFilenameWith ?? theEntry.FilenameToMatch;
            return true;
        }

        theNewName = null;
        return false;
    }
    
}