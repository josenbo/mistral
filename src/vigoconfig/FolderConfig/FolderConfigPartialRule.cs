using vigobase;

namespace vigoconfig;

internal class FolderConfigPartialRule
{
    public FileRuleActionEnum Action { get; set; } = FileRuleActionEnum.Undefined;
    public FileRuleConditionEnum Condition { get; set; } = FileRuleConditionEnum.Undefined;
    public string? CompareWith { get; set; } 
    public string? ReplaceWith { get; set; }
    public FolderConfigPartialHandling? Handling { get; set; } 

    public void Deconstruct(out FileRuleActionEnum action, out FileRuleConditionEnum condition, out string? compareWith, out string? replaceWith, out FolderConfigPartialHandling? handling)
    {
        action = Action;
        condition = Condition;
        compareWith = CompareWith;
        replaceWith = ReplaceWith;
        handling = Handling;
    }
}