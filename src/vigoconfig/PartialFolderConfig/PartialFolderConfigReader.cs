using System.Text;
using System.Text.RegularExpressions;
using Serilog;
using Serilog.Events;
using vigobase;

namespace vigoconfig;

internal static partial class PartialFolderConfigReader
{
    public static PartialFolderConfig Parse(
        string content, 
        string? configurationFile = null, 
        ConfigurationFileTypeEnum? configurationType = null)
    {
        var lines = content.Split(LineSeparators, StringSplitOptions.None).ToList();

        configurationFile ??= "anonymous text block";

        configurationType ??= GuessConfigurationTypeFromContent(lines);

        if (!configurationType.Value.IsDefinedAndValid())
        {
            Log.Error("The content of the folder configuration script '{TheConfigurationFile}' was not recognized as markdown, nor as native format", configurationFile);
            throw new VigoParseFolderConfigException($"The format of the folder configuration script '{configurationFile}' was not recognized");
        }
            
        var (folderBlock, ruleBlocks) = ReadSourceBlocks(lines, configurationFile, configurationType.Value);

        var folderConfig = new PartialFolderConfig();
        
        if (folderBlock is not null)
        {
            folderConfig.Block = folderBlock;
            var parser = new FolderBlockParser(folderConfig, folderBlock);
            parser.Parse();
        }

        foreach (var ruleBlock in ruleBlocks)
        {
            var partialRule = new PartialFolderConfigRule(ruleBlock);
            var parser = new RuleBlockParser(partialRule, ruleBlock);
            folderConfig.PartialRules.Add(partialRule);
            parser.Parse();
        }
        
        folderConfig.DumpToDebug();
        
        return folderConfig;
    }

    private static ConfigurationFileTypeEnum GuessConfigurationTypeFromContent(IReadOnlyCollection<string> lines)
    {
        if (ProbeMarkdown(lines))
            return ConfigurationFileTypeEnum.MarkdownFormat;

        return ProbeNative(lines) 
            ? ConfigurationFileTypeEnum.NativeFormat 
            : ConfigurationFileTypeEnum.Undefined;
    }

    private static (SourceBlockFolder? folderConfig,  IReadOnlyList<SourceBlockRule> rulesConfig) ReadSourceBlocks(
        IEnumerable<string> lines,
        string configurationFile,
        ConfigurationFileTypeEnum configurationType
        )
    {
        var blocks = GroupSourceBlocks(
            configurationFile,
            configurationType.IsMarkdownFormat() 
            ? GetSourceLinesFromMarkdown(lines) 
            : GetSourceLinesFromNative(lines));
        
        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Debug("Retrieved source lines from the {TheFormat} configuration '{TheConfigurationFile}' and grouped code blocks",
                configurationType.IsMarkdownFormat() ? "markdown" : "native",
                configurationFile);

            foreach (var block in blocks)
            {
                Log.Debug("Showing configuration block #{ThePosition} of type {TheBlockType}",
                    block.Position,
                    block.GetType().Name);
                
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

        var folderBlock = blocks.OfType<SourceBlockFolder>().SingleOrDefault();
        var ruleBlocks = blocks.OfType<SourceBlockRule>().ToList();

        if (ruleBlocks.Count == 0)
            return (folderBlock, ruleBlocks);

        var lastRuleBlock = ruleBlocks[0];
        for (var i = 1; i < ruleBlocks.Count; i++)
        {
            var currentRuleBlock = ruleBlocks[i];

            if (currentRuleBlock.Position <= lastRuleBlock.Position || 
                currentRuleBlock.FromLineNumber <= lastRuleBlock.ToLineNumber || 
                lastRuleBlock.ToLineNumber <= lastRuleBlock.FromLineNumber ||
                currentRuleBlock.ToLineNumber <= currentRuleBlock.FromLineNumber)
            {
                Log.Error("The order of configuration sections and source lines is not sequential");
                throw new VigoParseFolderConfigException("Could not establish the order of configuration sections");
            }

            lastRuleBlock = currentRuleBlock;
        }

        return (folderBlock, ruleBlocks);
    }

    private static IReadOnlyList<SourceBlock> GroupSourceBlocks(
        string configurationFile,
        IEnumerable<SourceLine> sourceLines)
    {
        var retval = new List<SourceBlock>();
        var tempLines = new List<SourceLine>();
        var position = 1;
        var ruleIndex = 1;
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
                sb.Append(sourceLine.Content).Append(('\n'));

                if (!RexEndBlock.IsMatch(sourceLine.Content)) 
                    continue;
                
                retval.Add(isRuleBlock
                    ? new SourceBlockRule(tempLines.ToList(), sb.ToString(), configurationFile, position++, ruleIndex++)
                    : new SourceBlockFolder(tempLines.ToList(), sb.ToString(), configurationFile, position++)
                );
                tempLines.Clear();
                sb.Clear();
                inBlock = false;
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
                sb.Append(sourceLine.Content).Append('\n');
            }
        }

        if (!inBlock) 
            return retval;
        
        
        Log.Error("The last block was not closed properly");
        throw new VigoParseFolderConfigException(
            $"Syntax error in the folder configuration. The block beginning at line {tempLines[0].LineNumber} was not closed. Expecting DONE.");
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
