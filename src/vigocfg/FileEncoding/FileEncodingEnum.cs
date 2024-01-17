using JetBrains.Annotations;

namespace vigocfg;

[PublicAPI]
public enum FileEncodingEnum
{
    Undefined = 0,
    // ReSharper disable InconsistentNaming
    ISO_8859_1 = 115001,
    Ascii = 115007,
    UTF_8 = 115008,
    ISO_8859_15 = 115015,
    Windows_1252 = 115052
    // ReSharper restore InconsistentNaming
}