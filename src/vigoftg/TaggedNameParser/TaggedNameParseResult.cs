// ReSharper disable NotAccessedPositionalProperty.Global

using vigobase;

namespace vigoftg;

internal record TaggedNameParseResult(
    string CurrentName, 
    bool HasTags, 
    bool HasScope, 
    string NewName, 
    bool IsInclusive, 
    IReadOnlyList<NamedTag> PrimaryTags, 
    IReadOnlyList<NamedTag> SecondaryTags, 
    IReadOnlyList<NamedTag> AdditionalTags);