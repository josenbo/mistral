using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using JetBrains.Annotations;
using Serilog;

namespace vigobase;

/// <summary>
/// Signaling that the default file permissions shall be changed as specified using symbolic notation
/// </summary>
/// <inheritdoc cref="FilePermissionType"/>
public partial record FilePermissionSymbolic(string TextRepresentation) : FilePermissionSpecified
{
    /// <summary>
    /// Gets the symbolic notation for modifying or setting file modes
    /// for user, group or others. See the chmod man pages on a unix box
    /// for further information. One example is u+x to grant the execute
    /// right to the user. Another one would be =rw to give user, group
    /// and others read and write permissions. You can describe multiple
    /// modifications by chaining expression with a comme (e.g.: u=rwx,g+x,o=)
    /// </summary>
    public override string TextRepresentation { get; protected set; } =
        Guard.Against.InvalidInput(TextRepresentation, nameof(TextRepresentation), IsValidSymbolicNotation);
    
    /// <inheritdoc cref="FilePermissionType"/>
    public override FilePermissionTypeEnum FilePermissionType => FilePermissionTypeEnum.Symbolic;
    
    /// <summary>
    /// Symbolic permission allow modifying only parts of the file mode,
    /// keeping the other parts unchanged. The given default file mode
    /// is taken as a starting point and then the modifications described
    /// in the symbolic notation are applied one after the other from left
    /// to right. After applying all modifications, the calculated file mode
    /// is returned. 
    /// </summary>
    /// <param name="defaultUnixFileMode">The file mode to build upon</param>
    /// <returns>The default with all changes applied</returns>
    /// <exception cref="VigoFatalException"></exception>
    /// <exception cref="ArgumentException"></exception>
    [PublicAPI]
    public override UnixFileMode ComputeUnixFileMode(UnixFileMode defaultUnixFileMode)
    {
        try
        {
            // The TextRepresentation is guaranteed to be a non-empty
            // and valid expression by the constructor checks.
            // So we can just split an process in confidence, throwing
            // an exception if things gow wrong
        
            var currentMode = (int)defaultUnixFileMode;

            foreach (var expr in TextRepresentation.Split(','))
            {
                var match = SymbolicTerm.Match(expr);
            
                if (!match.Success)
                {
                    Log.Error("Encountered the invalid expression {InvalidExpression} in the symbolic permission {TheSymbolicPermissionSpec}",
                        expr,
                        TextRepresentation);
                    throw new VigoFatalException(AppEnv.Faults.Fatal("FX525",null, $"The symbolic permission contains an invalid expression. Check the folder configuration file"));
                }
            
                // If the who group is empty or has
                // the value a, this is a short notation
                // for the combination ugo (user, group and others)
                // Since we have to handle these anyway, we 
                // omit the two edge cases by replacing the
                // term
                var whoGroup = match.Groups["who"].Value;
                if (string.IsNullOrEmpty(whoGroup) || whoGroup == "a")
                    whoGroup = "ugo";
            
                // The how group contains a single letter which is 
                // one of plus (+), minus (-) or equal (=)
                var howChar = match.Groups["how"].Value[0];
            
                // The what group represents a bitmask of
                // three bits for read, write and execute
                var whatGroup = match.Groups["what"].Value;

                var bitmask = 0;
            
                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                foreach (var whatChar in whatGroup)
                {
                    bitmask |= whatChar switch
                    {
                        'r' => 4,
                        'w' => 2,
                        'x' => 1,
                        _ => throw new ArgumentException(
                            $"Expecting only [rwx] but found '{whatChar}' while calculating the effective unix file mode for the symbolic expression {TextRepresentation}")
                    };
                }

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var whoChar in whoGroup)
                {
                    // updating the proper bits amounts to shifting the 
                    // bitmask by steps of three bits before applying
                    // the bitmask
                    var shift = whoChar switch
                    {
                        'u' => 6,
                        'g' => 3,
                        'o' => 0,
                        _ => throw new ArgumentException(
                            $"Expecting only [ugo] but found '{whoChar}' while calculating the effective unix file mode for the symbolic expression {TextRepresentation}")
                    };

                    var shiftedBitmask = bitmask << shift;
                
                    switch (howChar)
                    {
                        case '+':
                            // Easy: just bit-or
                            currentMode |= shiftedBitmask;
                            break;
                        case '-':
                            // A little more elaborate: inverting makes the 1's 
                            // in the bitmask become 0 and all others 1.
                            // Bit-and will thus just delete the 1's in the bitmask.
                            currentMode &= ~shiftedBitmask;
                            break;
                        case '=':
                            // To overwrite a three bit window but keep the rest, we
                            // set the bits in the window to 0 and the others to 1.
                            // Once this is done we can simply bit-or the bitmask in
                            // the emptied window.
                            // Example
                            // To update the group flags, we move the 111 mask 7 left 
                            // by 3 bits: 7 << 3. This gives 111000.
                            // Inverting this gives 11111111111111111111111111000111
                            // (Remember: this is a 32-bit integer)
                            // Bit-and this to delete the bits in the window and 
                            // keep all the other bits unchanged.
                            // The last step is to bit-or the desired bitmask in 
                            // the emptied window
                        
                            // shift the update window in the right position and invert 
                            var window = ~(7 << shift);
                            currentMode = (currentMode & window) | shiftedBitmask;
                            break;
                        default:
                            throw new ArgumentException(
                                $"Expecting only [-+=] but found '{howChar}' while calculating the effective unix file mode for the symbolic expression {TextRepresentation}");                        
                    } // switch (howChar)
                } // foreach (var whoChar
            } // foreach (var expr ...
        
            return (UnixFileMode)currentMode;
        }
        catch (Exception e) when (e is not VigoException)
        {
            Log.Error(e, "Could not calculate the unix file mode for the symbolic permission {TheSymbolicPermissionSpec} and the default file mode {DefaultFileMode}",
                TextRepresentation,
                defaultUnixFileMode);
            throw new VigoFatalException(AppEnv.Faults.Fatal("FX532","Check what happened and add proper handling for this case", string.Empty), e);
        }
    }

    private static readonly Regex SymbolicTerm = CompiledRegexSymbolicTerm();

    // ReSharper disable once StringLiteralTypo
    [GeneratedRegex("(?'who'[augo]{0,3})(?'how'[-+=])(?'what'[rwx]{0,3})", RegexOptions.None)]
    private static partial Regex CompiledRegexSymbolicTerm();
}