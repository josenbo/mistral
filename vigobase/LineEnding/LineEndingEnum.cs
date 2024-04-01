using JetBrains.Annotations;

namespace vigobase;

[PublicAPI]
public enum LineEndingEnum
{
    Undefined = 0,

    // ReSharper disable InconsistentNaming
    LF = 113001,
    // CR = 113002,
    CR_LF = 113003
    // ReSharper restore InconsistentNaming
}