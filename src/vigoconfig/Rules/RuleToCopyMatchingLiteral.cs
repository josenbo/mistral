using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal record RuleToCopyMatchingLiteral(
    string NameToMatch, 
    string NameReplacement,
    FileTypeEnum FileType,
    FileEncodingEnum SourceFileEncoding,
    FileEncodingEnum TargetFileEncoding,
    FilePermission FilePermission,
    LineEndingEnum LineEnding,
    bool FixTrailingNewline
) : RuleToCopyConditional(
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
    internal override bool GetTransformation(string filename, out RuleCheckResultEnum result,
        [NotNullWhen(true)] out IDeploymentTransformationReadWrite? transformation)
    {
        throw new NotImplementedException();
    }
}