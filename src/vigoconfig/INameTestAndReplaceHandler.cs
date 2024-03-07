using System.Diagnostics.CodeAnalysis;

namespace vigoconfig;

/// <summary>
/// A configurable file name matcher and provider
/// for replacement names
/// </summary>
public interface INameTestAndReplaceHandler
{
    /// <summary>
    /// Get a name which identifies the handler
    /// </summary>
    string Identification { get; }

    /// <summary>
    /// Tests a file name against a configurable definition
    /// and returns the new or old filename, depending on
    /// a replacement being defined or not. 
    /// </summary>
    /// <param name="theNameToTest">The filename which has to be checked against the definition</param>
    /// <param name="theNewName">Upon failure ths value is null. Otherwise, if a file name
    /// replacement is defined, this value will be returned. If there is no replacement,
    /// then the original file name is returned</param>
    /// <returns>True, if the file name is matched</returns>
    bool TestName(string theNameToTest, [NotNullWhen(true)] out string? theNewName);
}