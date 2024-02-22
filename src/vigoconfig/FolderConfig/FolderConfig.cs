using Serilog;
using Serilog.Events;

namespace vigoconfig;

internal class FolderConfig
{
    public bool? KeepEmptyFolder { get; set; }
    public FolderConfigPartialHandling? LocalDefaults { get; set; }
    public IList<FolderConfigPartialRule> PartialRules { get; } = [];
    
    public SourceBlockFolder? Block { get; set; }

    internal void DumpToDebug()
    {
        if (!Log.IsEnabled(LogEventLevel.Debug))
            return;
        
        Log.Debug("Showing the non-null properties in {TheObject}", nameof(FolderConfig));
        if (KeepEmptyFolder is not null)
            Log.Debug("{ThePropertyName} = {ThePropertyValue}", nameof(KeepEmptyFolder), KeepEmptyFolder);

        if (LocalDefaults is not null)
        {
            Log.Debug("Start of {ThePropertyName}", nameof(LocalDefaults));
            
            if (LocalDefaults.StandardModeForFiles is not null)
                Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(LocalDefaults.StandardModeForFiles), LocalDefaults.StandardModeForFiles);
            if (LocalDefaults.StandardModeForDirectories is not null)
                Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(LocalDefaults.StandardModeForDirectories), LocalDefaults.StandardModeForDirectories);
            if (LocalDefaults.FileType is not null)
                Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(LocalDefaults.FileType), LocalDefaults.FileType);
            if (LocalDefaults.SourceFileEncoding is not null)
                Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(LocalDefaults.SourceFileEncoding), LocalDefaults.SourceFileEncoding);
            if (LocalDefaults.TargetFileEncoding is not null)
                Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(LocalDefaults.TargetFileEncoding), LocalDefaults.TargetFileEncoding);
            if (LocalDefaults.LineEnding is not null)
                Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(LocalDefaults.LineEnding), LocalDefaults.LineEnding);
            if (LocalDefaults.Permissions is not null)
                Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(LocalDefaults.Permissions), LocalDefaults.Permissions);
            if (LocalDefaults.FixTrailingNewline is not null)
                Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(LocalDefaults.FixTrailingNewline), LocalDefaults.FixTrailingNewline);
            if (LocalDefaults.IsDefinedValidCharsRegex)
                Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(LocalDefaults.ValidCharsRegex), LocalDefaults.ValidCharsRegex);
            if (LocalDefaults.Targets is not null)
                Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(LocalDefaults.Targets), LocalDefaults.Targets);
            
            Log.Debug("End of {ThePropertyName}", nameof(LocalDefaults));
        }

        var ruleIndex = 0;
        
        foreach (var rule in PartialRules)
        {
            ruleIndex++;
            
            Log.Debug("Start of {ThePropertyName}[{TheRuleIndex}]", nameof(PartialRules), ruleIndex);
            
            Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.Action), rule.Action);
            Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.Condition), rule.Condition);

            if (rule.CompareWith is not null)
                Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.CompareWith), rule.CompareWith);
            if (rule.ReplaceWith is not null)
                Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.ReplaceWith), rule.ReplaceWith);
            
            if (rule.Handling is not null)
            {
                if (rule.Handling.StandardModeForFiles is not null)
                    Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.Handling.StandardModeForFiles), rule.Handling.StandardModeForFiles);
                if (rule.Handling.StandardModeForDirectories is not null)
                    Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.Handling.StandardModeForDirectories), rule.Handling.StandardModeForDirectories);
                if (rule.Handling.FileType is not null)
                    Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.Handling.FileType), rule.Handling.FileType);
                if (rule.Handling.SourceFileEncoding is not null)
                    Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.Handling.SourceFileEncoding), rule.Handling.SourceFileEncoding);
                if (rule.Handling.TargetFileEncoding is not null)
                    Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.Handling.TargetFileEncoding), rule.Handling.TargetFileEncoding);
                if (rule.Handling.LineEnding is not null)
                    Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.Handling.LineEnding), rule.Handling.LineEnding);
                if (rule.Handling.Permissions is not null)
                    Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.Handling.Permissions), rule.Handling.Permissions);
                if (rule.Handling.FixTrailingNewline is not null)
                    Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.Handling.FixTrailingNewline), rule.Handling.FixTrailingNewline);
                if (rule.Handling.IsDefinedValidCharsRegex)
                    Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.Handling.ValidCharsRegex), rule.Handling.ValidCharsRegex);
                if (rule.Handling.Targets is not null)
                    Log.Debug("    {ThePropertyName} = {ThePropertyValue}", nameof(rule.Handling.Targets), rule.Handling.Targets);
            }
            Log.Debug("End of {ThePropertyName}[{TheRuleIndex}]", nameof(PartialRules), ruleIndex);
        }
    }
}