using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using vigobase;

namespace vigoconfig;

internal record RuleToCopyMatchingPattern(
    int Index,
    string NameToMatch, 
    string NameReplacement,
    FileTypeEnum FileType,
    FileEncodingEnum SourceFileEncoding,
    FileEncodingEnum TargetFileEncoding,
    FilePermission FilePermission,
    LineEndingEnum LineEnding,
    bool FixTrailingNewline
) : RuleToCopyConditional(
    Index,
    NameToMatch, 
    NameReplacement,
    FileType,
    SourceFileEncoding,
    TargetFileEncoding,
    FilePermission,
    LineEnding,
    FixTrailingNewline
)
{
    internal override bool GetTransformation(FileInfo file,
        DeploymentDefaults defaults,
        [NotNullWhen(true)] out IDeploymentTransformationReadWriteFile? transformation)
    {
        if (!Regex.IsMatch(file.Name, NameToMatch))
        {
            transformation = null;
            return false;
        }

        var newName = string.IsNullOrWhiteSpace(NameReplacement)
            ? string.Empty
            : Regex.Replace(file.Name, NameToMatch, NameReplacement);

        transformation = new DeploymentTransformationFile(file, defaults)
        {
            CanDeploy = true,
            DifferentTargetFileName = newName,
            FileType = FileType,
            SourceFileEncoding = SourceFileEncoding,
            TargetFileEncoding = TargetFileEncoding,
            FilePermission = FilePermission,
            LineEnding = LineEnding,
            FixTrailingNewline = FixTrailingNewline
        };
        return true;
    }
}