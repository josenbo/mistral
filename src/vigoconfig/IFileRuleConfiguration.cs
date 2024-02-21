using JetBrains.Annotations;
using vigobase;

namespace vigoconfig;

/// <summary>
/// A deployment rule describing file properties and
/// parameters used to check and transform the file
/// during deployments
/// </summary>
[PublicAPI]
public interface IFileRuleConfiguration
{
    /// <summary>
    /// The position of the ruleset which is derived from
    /// the order of rules in the deployment configuration
    /// file. Rules are always applied in order up to and
    /// including the first matching rule.  
    /// </summary>
    int RuleIndex { get; }
    
    /// <summary>
    /// A textual description of the rule's action and the
    /// condition for checking if the rule matches a file. 
    /// </summary>
    string RuleDescription { get;  }
    
    /// <summary>
    /// The action to be performed on the file(s), when the
    /// rule matches.
    /// </summary>
    FileRuleActionEnum Action { get; }
    
    /// <summary>
    /// The type of condition to check in order to decide
    /// if a file matches the rule
    /// </summary>
    FileRuleConditionEnum Condition { get; }
    
    /// <summary>
    /// Depending in the type of condition, this may contain
    /// a file name or a regular expression for matching
    /// filenames. The value is null or empty for
    /// unconditional rules, otherwise it guaranteed to
    /// be non-empty.
    /// </summary>
    string? CompareWith { get; }
    
    /// <summary>
    /// An optional specification for finding the new 
    /// new file name and renaming the matching files.
    /// If null, the matching files shall not be renamed.
    /// It is not used for unconditional rules.
    /// </summary>
    string? ReplaceWith { get; }

    /// <summary>
    /// Properties of the file and parameters used
    /// to check and transform the file during
    /// deployment.
    /// </summary>
    FileHandlingParameters Handling { get; }

    /// <summary>
    /// True, if the rule is based on an explicit
    /// definition in the configuration file. This
    /// is evidently the case for all rules read
    /// from the configuration file. Only for
    /// derived additional rules like 'the skip
    /// the rest rule' would this be false.
    /// </summary>
    bool IsExplicitelyDefined { get; }

    /// <summary>
    /// If the rule was explicitely defined in
    /// the configuration file, this content can
    /// be accessed here. There may be implicit
    /// rules for which this information is not
    /// available.
    /// </summary>
    string? BasedOnTheConfigurationText { get; }
}