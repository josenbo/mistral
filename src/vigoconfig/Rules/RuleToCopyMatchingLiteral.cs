using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal record RuleToCopyMatchingLiteral(
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
        if (!NameToMatch.Equals(file.Name, StringComparison.Ordinal))
        {
            transformation = null;
            return false;
        }

        var newName = string.IsNullOrWhiteSpace(NameReplacement)
            ? string.Empty
            : NameReplacement;

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