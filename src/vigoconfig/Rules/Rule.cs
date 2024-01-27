using System.Diagnostics.CodeAnalysis;
using vigobase;

namespace vigoconfig;

internal abstract record Rule()
{
    internal abstract bool GetTransformation(string filename, out RuleCheckResultEnum result, [NotNullWhen(true)]  out IDeploymentTransformationReadWrite? transformation);
}