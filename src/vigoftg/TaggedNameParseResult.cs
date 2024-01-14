// ReSharper disable NotAccessedPositionalProperty.Global

namespace vigoftg;

internal record TaggedNameParseResult(string CurrentName, bool HasTags, bool HasScope, string NewName, bool IsInclusive, IReadOnlyList<string> PrimaryTags, IReadOnlyList<string> SecondaryTags, IReadOnlyList<string> AdditionalTags);