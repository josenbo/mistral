using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal record RuleToCopyUnconditional(
    int Index,
    FileTypeEnum FileType,
    FileEncodingEnum SourceFileEncoding,
    FileEncodingEnum TargetFileEncoding,
    FilePermission FilePermission,
    LineEndingEnum LineEnding,
    bool FixTrailingNewline
) : RuleToCopy(
    Index,
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
        transformation = new DeploymentTransformationFile(file, defaults)
        {
            CanDeploy = true,
            DifferentTargetFileName = string.Empty,
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