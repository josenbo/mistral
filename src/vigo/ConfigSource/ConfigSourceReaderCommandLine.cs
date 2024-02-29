using Serilog;
using vigobase;

namespace vigo;

internal class ConfigSourceReaderCommandLine : IConfigSourceReader
{
    public AppArguments Read(AppArguments initial)
    {
        return Parse(initial, _cmdArgs);
    }

    public ConfigSourceReaderCommandLine() : this(Environment.GetCommandLineArgs())
    {
    }

    public ConfigSourceReaderCommandLine(IEnumerable<string> args)
    {
        _cmdArgs.AddRange(args);
    }

    private static AppArguments Parse(AppArguments initial, IReadOnlyList<string> cmdArgs)
    {
        var cmdArgsWithoutOptions = new List<string>();

        Log.Debug("Parsing the command line {TheCommandLine}", cmdArgs);
        
        var result = ExtractOptionsAndWords(initial, cmdArgs, cmdArgsWithoutOptions);

        Log.Debug("Extracted the program options {TheOptions} and the still unmatched arguments {TheArguments}",
            result,
            cmdArgsWithoutOptions);
        
        var firstCmdArg = 0 < cmdArgsWithoutOptions.Count
            ? cmdArgsWithoutOptions[0]
            : string.Empty;

        var command = firstCmdArg.ToLowerInvariant() switch
        {
            "deploy" => CommandEnum.Deploy,
            "check" => CommandEnum.Check,
            "explain" => CommandEnum.Explain,
            _ => CommandEnum.Undefined
        };
        
        Log.Debug("Parsed the action {TheAction} from the first command line argument {TheFirst}",
            command,
            firstCmdArg);
        
        if (result.ShowHelp.HasValue && result.ShowHelp.Value)
        {
            Log.Debug("Prioritizing the HELP action with the help context {TheContext}", command);
            
            return result with
            {
                Command = CommandEnum.Help,
                CommandToShowHelpFor = command
            };
        }

        if (result.ShowVersion.HasValue && result.ShowVersion.Value)
        {
            Log.Debug("Prioritizing the VERSION action");

            return result with
            {
                Command = CommandEnum.Version
            };
        }
        
        if (cmdArgsWithoutOptions.Count == 0)
            throw new VigoFatalException(AppEnv.Faults.Fatal("No command specified"));

        if (command == CommandEnum.Undefined)
        {
            if (string.IsNullOrWhiteSpace(firstCmdArg) || !File.Exists(firstCmdArg))
                throw new VigoFatalException(AppEnv.Faults.Fatal("No command specified"));

            Log.Debug("This might be the command line that you get from the shell, when you run the configuration file as a script CONFIGURATION-FILE [Name 1] .. [Name n]. Will translate this to the corresponding EXPLAIN action.");
            
            var configurationFile = new FileInfo(Path.GetFullPath(firstCmdArg));
            
            if (!configurationFile.Exists || configurationFile.Directory is null || !configurationFile.Directory.Exists)
                throw new VigoFatalException(
                    AppEnv.Faults.Fatal("An invalid configuration file was passed on the command line"));

            var names = new List<string>();

            for (var i = 1; i < cmdArgsWithoutOptions.Count; i++)
            {
                var name = Path.GetFileName(cmdArgsWithoutOptions[i]);
                if (string.IsNullOrWhiteSpace(name))
                {
                    Log.Warning(
                        "Ignoring the name parameter on the command line, because it is not a valid file name: {TheValue}",
                        cmdArgsWithoutOptions[i]);
                }
                else
                {
                    if (name != cmdArgsWithoutOptions[i])
                        Log.Warning("Dropping the path from the name parameter on the command line: New is {TheNewValue}, old was {TheOldValue}", 
                            name,
                            cmdArgsWithoutOptions[i]);

                    names.Add(name);
                }
            }
            
            Log.Debug("Transformed the shell command line to an EXPLAIN action. Configuration file is {TheConfigFile} and names are {TheNames}",
                configurationFile,
                names);

            result = result with
            {
                ConfigurationFile = configurationFile,
                Names = names
            };
        }
        else if (1 < cmdArgsWithoutOptions.Count)
        {
            var unexpected = cmdArgsWithoutOptions[1..];
            Log.Error("Unexpected command line arguments {TheUnexpectedArguments}", unexpected);
            throw new VigoFatalException(AppEnv.Faults.Fatal("Encountered unexpected command line arguments"));
        }
        else
        {
            Log.Debug("Setting the action {TheAction} after handling all edge cases", command);
            
            result = result with { Command = command };
        }
        
        return result;
    }
    
    private static AppArguments ExtractOptionsAndWords(AppArguments initial, IReadOnlyList<string> cmdArgs, List<string> cmdArgsWithoutOptions)
    {
        var retval = initial;
        
        var options = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        for (var i = 0; i < cmdArgs.Count; i++)
        {
            var current = cmdArgs[i];
            var next = (i + 1) < cmdArgs.Count ? cmdArgs[i + 1] : null;

            if (MatchesOneOf(options, "H", current, "-h", "--help"))
            {
                Log.Debug("Saw the HELP command line option {TheOption}", current);
                
                if (retval.ShowHelp.HasValue)
                    continue;
                retval = retval with { ShowHelp = true };
            }
            else if (MatchesOneOf(options, "V", current, "-v", "--version"))
            {
                Log.Debug("Saw the VERSION command line option {TheOption}", current);
                
                if (retval.ShowVersion.HasValue)
                    continue;
                retval = retval with { ShowVersion = true };
            }
            else if (MatchesOneOf(options, "R", current, "-r", "--repository-root"))
            {
                Log.Debug("Saw the REPOSITORY-ROOT command line option {TheOption} with the value {TheValue}", current, next);

                i++; // eat the next argument
                
                if (retval.RepositoryRoot is not null)
                    continue;
                
                if (string.IsNullOrEmpty(next))
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The value for -r or --repository-root is missing"));

                if (string.IsNullOrEmpty(next) || !Directory.Exists(next))
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The value for -r or --repository-root must be an existing directory"));

                var repositoryRoot = new DirectoryInfo(Path.GetFullPath(next));
                
                retval = retval with { RepositoryRoot = repositoryRoot };
            }
            else if (MatchesOneOf(options, "O", current, "-o", "--output-file"))
            {
                Log.Debug("Saw the OUTPUT-FILE command line option {TheOption} with the value {TheValue}", current, next);

                i++; // eat the next argument
                
                if (retval.OutputFile is not null)
                    continue;
                
                if (string.IsNullOrEmpty(next))
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The value for -o or --output-file is missing"));

                var deploymentBundle = new FileInfo(Path.GetFullPath(next));
                
                if (deploymentBundle.Directory is null || !deploymentBundle.Directory.Exists)
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The value for -o or --output-file must be file in an existing directory"));

                retval = retval with { OutputFile = deploymentBundle };
            }
            else if (MatchesOneOf(options, "T", current, "-t", "--targets"))
            {
                Log.Debug("Saw the TARGETS command line option {TheOption} with the value {TheValue}", current, next);

                i++; // eat the next argument
                
                if (retval.Targets is not null)
                    continue;
                
                if (string.IsNullOrEmpty(next))
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The value for -t or --targets is missing"));

                var targets = next.Split(ListSeparators,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

                if (0 == targets.Count)
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The value for -t or --targets must be a list of target names"));

                var invalidTargets = targets.Where(t => !DeploymentTargetHelper.IsValidName(t)).ToList();
                
                if (0 < invalidTargets.Count)
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The option -t or --targets has invalid target names {string.Join(", ", invalidTargets)}"));

                retval = retval with { Targets = targets };
            }
            else if (MatchesOneOf(options, "C", current, "-c", "--configuration-file"))
            {
                Log.Debug("Saw the CONFIGURATION-FILE command line option {TheOption} with the value {TheValue}", current, next);

                i++; // eat the next argument
                
                if (retval.ConfigurationFile is not null)
                    continue;
                
                if (string.IsNullOrEmpty(next))
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The value for -c or --configuration-file is missing"));

                var configurationFile = new FileInfo(Path.GetFullPath(next));
                
                if (!configurationFile.Exists || configurationFile.Directory is null || !configurationFile.Directory.Exists)
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The value for -c or --configuration-file must be an existing file"));

                retval = retval with { ConfigurationFile = configurationFile };
            }
            else if (MatchesOneOf(options, "N", current, "-n", "--names"))
            {
                Log.Debug("Saw the NAMES command line option {TheOption} with the value {TheValue}", current, next);

                i++; // eat the next argument
                
                if (retval.Names is not null)
                    continue;
                
                if (string.IsNullOrEmpty(next))
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The value for -n or --names is missing"));

                var names = next.Split(ListSeparators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(Path.GetFileName)
                    .OfType<string>()
                    .ToList();

                if (0 == names.Count)
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The value for -n or --names must be a list of filenames"));

                retval = retval with { Names = names };
            }
            else if (current.StartsWith('-'))
            {
                Log.Debug("Saw an unknown command line option {TheOption}", current);

                throw new VigoFatalException(AppEnv.Faults.Fatal($"Unknown command line option {current}"));
            }
            else
            {
                if (AppEnv.IsApplicationModulePath(current))
                {
                    Log.Debug("Ignoring a command line parameter that points to a module of the current process. Skipped module path is {TheModulePath}", 
                        current);
                    
                    continue;
                }
                
                Log.Debug("Registered a command line argument for handling in the next step {TheArg}", current);

                cmdArgsWithoutOptions.Add(current);
            }
        }

        return retval;
    }
    
    private static bool MatchesOneOf(Dictionary<string, string> dict, string key, string opt, params string[] opts)
    {
        if (!opts.Any(o => o.Equals(opt, StringComparison.InvariantCultureIgnoreCase)))
            return false;

        if (dict.TryAdd(key, key)) 
            return true;
        
        Log.Error("The command line options {Options} can appear only once", opts);
        throw new VigoFatalException(AppEnv.Faults.Fatal($"A command line option appears twice"));
    }
    
    private readonly List<string> _cmdArgs = [];
    
    private static readonly char[] ListSeparators = new char[] { ' ', ',', ':' };
}