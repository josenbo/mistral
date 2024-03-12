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
        
        var vigoMarkerLine = lines.FirstOrDefault(l => l.TrimStart().StartsWith("vîgô"));

        if (vigoMarkerLine is null)
        {
            Log.Fatal("The content of the folder configuration script '{TheConfigurationFile}' does not contain the vîgô marker",
                configurationFile);
            throw new VigoFatalException(AppEnv.Faults.Fatal(
                "FX133",
                "Configuration files must have the term vîgô appear as the first configuration statement",
                $"A folder configuration script is missing the vîgô marker. See log for details"));
        }
        
        if (configurationType.HasValue && !configurationType.Value.IsDefinedAndValid())
            configurationType = null;
            
        configurationType ??= GuessConfigurationType(configurationFile, lines);

        if (!configurationType.Value.IsDefinedAndValid())
        {
            Log.Fatal("The content of the folder configuration script '{TheConfigurationFile}' was not recognized as markdown, nor as native format", configurationFile);
            throw new VigoFatalException(AppEnv.Faults.Fatal(
                "FX140",
                null,
                "Unrecognized format of a folder configuration script. See log for details"));
            // throw new VigoParseFolderConfigException($"The format of the folder configuration script '{configurationFile}' was not recognized");
        }
            
        var (folderBlock, ruleBlocks, defineBlocks) = ReadSourceBlocks(lines, configurationFile, configurationType.Value);

        var commonDefinitions = new CommonDefinitions();

        foreach (var defineBlock in defineBlocks)
        {
            var parser = new DefineBlockParser(commonDefinitions, defineBlock);
            parser.Parse();
        }
        
        var folderConfig = new PartialFolderConfig(configurationType.Value);
        
        if (folderBlock is not null)
        {
            folderConfig.Block = folderBlock;
            var parser = new FolderBlockParser(folderConfig, folderBlock);
            parser.Parse();
        }

        foreach (var ruleBlock in ruleBlocks)
        {
            var partialRule = new PartialFolderConfigRule(ruleBlock);
            var parser = new RuleBlockParser(partialRule, ruleBlock, commonDefinitions);
            folderConfig.PartialRules.Add(partialRule);
            parser.Parse();
        }
        
        folderConfig.DumpToDebug();
        
        return folderConfig;
    }

    private static ConfigurationFileTypeEnum GuessConfigurationType(string configurationFile, List<string> lines)
    {
        switch (Path.GetExtension(configurationFile).ToLowerInvariant())
        {
            case ".md":
                return ConfigurationFileTypeEnum.MarkdownFormat;
            case ".vigo":
                return ConfigurationFileTypeEnum.NativeFormat;
        }

        if (ProbeMarkdown(lines))
            return ConfigurationFileTypeEnum.MarkdownFormat;

        return ProbeNative(lines)
            ? ConfigurationFileTypeEnum.NativeFormat
            : ConfigurationFileTypeEnum.Undefined;
    }

    private static (SourceBlockFolder? folderConfig,  IReadOnlyList<SourceBlockRule> rulesConfig,  IReadOnlyList<SourceBlockDefine> defineConfig) ReadSourceBlocks(
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
            throw new VigoFatalException(AppEnv.Faults.Fatal(
                "FX161",
                null,
                "Failed to parse a folder configuration script. See log for details"));
        }

        var folderBlock = blocks.OfType<SourceBlockFolder>().SingleOrDefault();
        var ruleBlocks = blocks.OfType<SourceBlockRule>().ToList();
        var defineBlocks = blocks.OfType<SourceBlockDefine>().ToList();

        if (ruleBlocks.Count == 0)
            return (folderBlock, ruleBlocks, defineBlocks);

        var lastRuleBlock = ruleBlocks[0];
        for (var i = 1; i < ruleBlocks.Count; i++)
        {
            var currentRuleBlock = ruleBlocks[i];

            if (currentRuleBlock.Position <= lastRuleBlock.Position || 
                currentRuleBlock.FromLineNumber <= lastRuleBlock.ToLineNumber || 
                lastRuleBlock.ToLineNumber <= lastRuleBlock.FromLineNumber ||
                currentRuleBlock.ToLineNumber <= currentRuleBlock.FromLineNumber)
            {
                Log.Fatal("The order of configuration sections and source lines is not sequential");
                throw new VigoFatalException(AppEnv.Faults.Fatal(
                    "FX168",
                    "Blocks are numbered sequentially and do not overlap",
                    string.Empty));
            }

            lastRuleBlock = currentRuleBlock;
        }

        return (folderBlock, ruleBlocks, defineBlocks);
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
        // var inBlock = false;
        var currentBlockType = CodeBlockTypeEnum.Undefined;
        // var isRuleBlock = false;
        var isFirstStatement = true;

        foreach (var sourceLine in sourceLines)
        {
            if (string.IsNullOrWhiteSpace(sourceLine.Content))
                continue;

            if (isFirstStatement)
            {
                if (!sourceLine.Content.TrimStart().StartsWith("vîgô"))
                {
                    Log.Fatal("The configuration does not start with the vîgô marker. {TheFile} {TheLine}",
                        configurationFile,
                        sourceLine);
                    
                    throw new VigoFatalException(AppEnv.Faults.Fatal(
                        "FX217",
                        "The first configuration statement must be a line starting with the term vîgô",
                        "Could not read the folder configuration. See log for details."));
                }

                isFirstStatement = false;
                continue;
            }
            
            if (currentBlockType != CodeBlockTypeEnum.Undefined)
            {
                tempLines.Add(sourceLine);
                sb.Append(sourceLine.Content).Append(('\n'));

                if (!RexEndBlock.IsMatch(sourceLine.Content)) 
                    continue;

                SourceBlock sourceBlock = currentBlockType switch
                {
                    CodeBlockTypeEnum.Undefined => throw new VigoFatalException(AppEnv.Faults.Fatal(
                        "FX224", 
                        "We should never get here", 
                        string.Empty)),
                    
                    CodeBlockTypeEnum.FolderBlock => new SourceBlockFolder(
                        tempLines.ToList(),
                        sb.ToString(),
                        configurationFile, 
                        position++),
                    
                    CodeBlockTypeEnum.RuleBlock => new SourceBlockRule(
                        tempLines.ToList(), 
                        sb.ToString(),
                        configurationFile,
                        position++, 
                        ruleIndex++),
                    
                    CodeBlockTypeEnum.DefineBlock => new SourceBlockDefine(
                        tempLines.ToList(), 
                        sb.ToString(), 
                        configurationFile,
                        position++),
                    
                    _ => throw new VigoFatalException(AppEnv.Faults.Fatal(
                        "FX231", 
                        "Is there a new block type which we forgot to implement?", 
                        string.Empty))
                };
                
                retval.Add( sourceBlock);
                
                tempLines.Clear();
                sb.Clear();
                currentBlockType = CodeBlockTypeEnum.Undefined;
            }
            else
            {
                if (RexBeginRuleBlock.IsMatch(sourceLine.Content))
                {
                    currentBlockType = CodeBlockTypeEnum.RuleBlock;
                }
                else if (RexBeginFolderBlock.IsMatch(sourceLine.Content))
                {
                    currentBlockType = CodeBlockTypeEnum.FolderBlock;
                }
                else if (RexBeginDefineBlock.IsMatch(sourceLine.Content))
                {
                    currentBlockType = CodeBlockTypeEnum.DefineBlock;
                }
                else
                {
                    Log.Fatal("Expecting DO, CONFIGURE or DEFINE, but found unmatched token in line {TheLine}", sourceLine);
                    throw new VigoFatalException(AppEnv.Faults.Fatal(
                        "FX175",
                        null,
                        "Failed to parse a folder configuration script. See log for details"));
                    
                }
                tempLines.Clear();
                tempLines.Add(sourceLine);
                sb.Clear();
                sb.Append(sourceLine.Content).Append('\n');
            }
        }

        if (currentBlockType == CodeBlockTypeEnum.Undefined) 
            return retval;
        
        
        Log.Fatal("Syntax error in the folder configuration '{TheConfigFile}'. The block beginning at line {TheFirstLine} was not closed. Expecting DONE",
            configurationFile,
            tempLines[0].LineNumber);
        throw new VigoFatalException(AppEnv.Faults.Fatal(
            "FX182",
            null,
            "Failed to parse a folder configuration script. See log for details"));
    }

    private static bool ProbeMarkdown(IEnumerable<string> lines)
    {
        var markdownLines = GetSourceLinesFromMarkdown(lines);
        if (markdownLines.Count < 1)
            return false;
        var firstLine = markdownLines[0].Content.Trim();
        if (!firstLine.StartsWith("vîgô"))
            return false;
        
        Log.Debug("Recognized markdown format");
        return true;
    }

    private static bool ProbeNative(IEnumerable<string> lines)
    {
        var nativeLines = GetSourceLinesFromNative(lines);
        if (nativeLines.Count < 1)
            return false;
        var firstLine = nativeLines[0].Content.Trim();
        if (!firstLine.StartsWith("vîgô"))
            return false;
        
        Log.Debug("Recognized native format");
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
    private static readonly Regex RexBeginMarkdownCodeBlock = RexBeginMarkdownCodeBlockCompiled();
    private static readonly Regex RexEndMarkdownCodeBlock = RexEndMarkdownCodeBlockCompiled();
    private static readonly Regex RexEmptyOrComment = RexEmptyOrCommentCompiled();
    private static readonly Regex RexBeginRuleBlock = RexBeginRuleBlockCompiled();
    private static readonly Regex RexBeginFolderBlock = RexBeginFolderBlockCompiled();
    private static readonly Regex RexBeginDefineBlock = RexBeginDefineBlockCompiled();
    private static readonly Regex RexEndBlock = RexEndBlockCompiled();
    
    #region Generated Embedded Regex
    
    [GeneratedRegex(@"^[ \t]*#.*vîgô.*$")]
    private static partial Regex RexMarkdownPatternCompiled();
    
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
    
    [GeneratedRegex(@"^\s*define\s.*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RexBeginDefineBlockCompiled();

    [GeneratedRegex(@"^\s*do\s.*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RexBeginRuleBlockCompiled();
    
    #endregion

    private enum CodeBlockTypeEnum
    {
        Undefined = 0,
        FolderBlock = 173301,
        RuleBlock = 629893,
        DefineBlock = 447373
    }
    
    private static readonly string[] LineSeparators = new[] { "\r\n", "\n" };
}