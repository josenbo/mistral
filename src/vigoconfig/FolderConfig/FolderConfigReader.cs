using System.Text;
using System.Text.RegularExpressions;
using Serilog;
using Serilog.Events;

namespace vigoconfig;

internal static partial class FolderConfigReader
{
    public static FolderConfig Parse(string content)
    {
        var (folderBlock, ruleBlocks) = ReadSourceBlocks(content);

        var folderConfig = new FolderConfig();
        
        if (folderBlock is not null)
        {
            var parser = new FolderBlockParser(folderConfig, folderBlock);
            parser.Parse();
        }

        foreach (var ruleBlock in ruleBlocks)
        {
            var partialRule = new FolderConfigPartialRule();
            var parser = new RuleBlockParser(partialRule, ruleBlock);
            folderConfig.PartialRules.Add(partialRule);
            parser.Parse();
        }
        
        folderConfig.DumpToDebug();
        
        return folderConfig;
    }

    private static (SourceBlockFolder? folderConfig,  IReadOnlyList<SourceBlockRule> rulesConfig) ReadSourceBlocks(string content)
    {
        var lines = content.Split(LineSeparators, StringSplitOptions.None).ToArray();

        var isMarkdown = false;

        if (ProbeMarkdown(lines))
            isMarkdown = true;
        else if (!ProbeNative(lines))
        {
            Log.Error("The content of the folder configuration script was not recognized as markdown, nor as native format");
            throw new VigoParseFolderConfigException("The format of the folder configuration script was not recognized");
        }

        // var sourceLines = isMarkdown ? GetSourceLinesFromMarkdown(lines) : GetSourceLinesFromNative(lines);
        //
        // if (Log.IsEnabled(LogEventLevel.Debug))
        // {
        //     Log.Debug("Retrieved source lines from {TheFormat} format",
        //         isMarkdown ? "markdown" : "native");
        //     foreach (var sourceLine in sourceLines)
        //     {
        //         Log.Debug("{TheLineNumber,-3}: {TheLine}",
        //             sourceLine.LineNumber,
        //             sourceLine.Content);
        //     }
        // }

        var blocks = GroupSourceBlocks(isMarkdown ? GetSourceLinesFromMarkdown(lines) : GetSourceLinesFromNative(lines));
        
        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Debug("Retrieved source lines from {TheFormat} format and grouped code blocks",
                isMarkdown ? "markdown" : "native");

            foreach (var block in blocks)
            {
                Log.Debug("Showing the {TheBlockType}", block.GetType().Name);
                
                foreach (var sourceLine in block.Lines)
                {
                    Log.Debug("{TheLineNumber,-3}: {TheLine}",
                        sourceLine.LineNumber,
                        sourceLine.Content);
                }
            }
        }

        if (1 < blocks.OfType<SourceBlockFolder>().Count())
        {
            Log.Error("There is more than one folder configuration block");
            throw new VigoParseFolderConfigException("There can be no more than a single folder configuration block");
        }

        return (blocks.OfType<SourceBlockFolder>().SingleOrDefault(), blocks.OfType<SourceBlockRule>().ToList());
    }

    private static IReadOnlyList<SourceBlock> GroupSourceBlocks(IEnumerable<SourceLine> sourceLines)
    {
        var retval = new List<SourceBlock>();
        var tempLines = new List<SourceLine>();
        var sb = new StringBuilder();
        var inBlock = false;
        var isRuleBlock = false;

        foreach (var sourceLine in sourceLines)
        {
            if (string.IsNullOrWhiteSpace(sourceLine.Content))
                continue;
            
            if (inBlock)
            {
                tempLines.Add(sourceLine);
                sb.AppendLine(sourceLine.Content);
                
                if (RexEndBlock.IsMatch(sourceLine.Content))
                {
                    retval.Add(isRuleBlock
                        ? new SourceBlockRule(tempLines.ToList(), sb.ToString())
                        : new SourceBlockFolder(tempLines.ToList(), sb.ToString())
                    );
                    tempLines.Clear();
                    sb.Clear();
                    inBlock = false;
                }
            }
            else
            {
                if (RexBeginRuleBlock.IsMatch(sourceLine.Content))
                {
                    inBlock = true;
                    isRuleBlock = true;
                }
                else if (RexBeginFolderBlock.IsMatch(sourceLine.Content))
                {
                    inBlock = true;
                    isRuleBlock = false;
                }
                else
                {
                    Log.Error("Unexpected token in line {TheLine}", sourceLine);
                    throw new VigoParseFolderConfigException(
                        $"Syntax error in the folder configuration at line {sourceLine.LineNumber}. Expecting DO or CONFIGURE.");
                }
                tempLines.Clear();
                tempLines.Add(sourceLine);
                sb.Clear();
                sb.AppendLine(sourceLine.Content);
            }
        }

        if (inBlock)
        {
            Log.Error("The last block was not closed properly");
            throw new VigoParseFolderConfigException(
                $"Syntax error in the folder configuration. The block beginning at line {tempLines[0].LineNumber} was not closed. Expecting DONE.");
        }

        return retval;
    }

    private static bool ProbeMarkdown(IEnumerable<string> lines)
    {
        var line = lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
        if (line is null || !RexMarkdownPattern.IsMatch(line)) return false;
        Log.Debug("Recognized markdown format");
        return true;
    }

    private static IReadOnlyList<SourceLine> GetSourceLinesFromMarkdown(IEnumerable<string> lines)
    {
        var retval = new List<SourceLine>();
        var inBlock = false;
        var lineNumber = 0;
        
        foreach (var line in lines)
        {
            lineNumber++;

            if (inBlock)
            {
                if (RexEndMarkdownCodeBlock.IsMatch(line))
                {
                    inBlock = false;
                    continue;
                }
                if (RexEmptyOrComment.IsMatch(line))
                    continue;
                retval.Add(new SourceLine(lineNumber, line));
            }
            else
            {
                if (RexBeginMarkdownCodeBlock.IsMatch(line))
                    inBlock = true;
            }
        }

        return retval;
    }
    
    private static bool ProbeNative(IEnumerable<string> lines)
    {
        var first = true;
        foreach (var line in lines)
        {
            if (first)
            {
                if (!RexShebangPattern.IsMatch(line))
                    return false;
                else
                    Log.Debug("Shebang found");
                
                first = false;
            }
            else
            {
                if (!RexNativePattern.IsMatch(line)) continue;
                
                Log.Debug("Recognized native format");
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyList<SourceLine> GetSourceLinesFromNative(IEnumerable<string> lines)
    {
        var retval = new List<SourceLine>();
        var lineNumber = 0;
        
        foreach (var line in lines)
        {
            lineNumber++;

            if (RexEmptyOrComment.IsMatch(line))
                continue;
            retval.Add(new SourceLine(lineNumber, line));
        }

        return retval;
    }
    
    private static readonly Regex RexMarkdownPattern = RexMarkdownPatternCompiled();
    private static readonly Regex RexNativePattern = RexNativePatternCompiled();
    private static readonly Regex RexShebangPattern = RexShebangPatternCompiled();
    private static readonly Regex RexBeginMarkdownCodeBlock = RexBeginMarkdownCodeBlockCompiled();
    private static readonly Regex RexEndMarkdownCodeBlock = RexEndMarkdownCodeBlockCompiled();
    private static readonly Regex RexEmptyOrComment = RexEmptyOrCommentCompiled();
    private static readonly Regex RexBeginRuleBlock = RexBeginRuleBlockCompiled();
    private static readonly Regex RexBeginFolderBlock = RexBeginFolderBlockCompiled();
    private static readonly Regex RexEndBlock = RexEndBlockCompiled();

    #region Generated Embedded Regex
    
    [GeneratedRegex(@"^[ \t]*#.*vîgô.*$")]
    private static partial Regex RexMarkdownPatternCompiled();

    [GeneratedRegex(@"^[ \t]*#[ \t]*vîgô.*$")]
    private static partial Regex RexNativePatternCompiled();
    
    [GeneratedRegex(@"^#!/usr/bin/env[ \t]{1,8}vigo[ \t]*$")]
    private static partial Regex RexShebangPatternCompiled();
    
    [GeneratedRegex(@"^[ \t]{0,8}```[ \t]{0,4}vigo\b")]
    private static partial Regex RexBeginMarkdownCodeBlockCompiled();
    
    [GeneratedRegex(@"^[ \t]{0,8}```[ \t]*$")]
    private static partial Regex RexEndMarkdownCodeBlockCompiled();

    [GeneratedRegex(@"^((\s*)|(\s*#.*))$")]
    private static partial Regex RexEmptyOrCommentCompiled();

    [GeneratedRegex(@"^\s*configure\s.*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RexBeginFolderBlockCompiled();
    
    [GeneratedRegex(@"^\s*done\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RexEndBlockCompiled();
    
    [GeneratedRegex(@"^\s*do\s.*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RexBeginRuleBlockCompiled();
    
    #endregion

    private static readonly string[] LineSeparators = new[] { "\r\n", "\n" };
}
