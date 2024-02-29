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

        var result = ExtractOptionsAndWords(initial, cmdArgs, cmdArgsWithoutOptions);

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
        
        if (result.ShowHelp.HasValue && result.ShowHelp.Value)
        {
            return result with
            {
                Command = CommandEnum.Help,
                CommandToShowHelpFor = command
            };
        }

        if (result.ShowVersion.HasValue && result.ShowVersion.Value)
        {
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

            var configurationFile = new FileInfo(Path.GetFullPath(firstCmdArg));
            
            if (!configurationFile.Exists || configurationFile.Directory is null || !configurationFile.Directory.Exists)
                throw new VigoFatalException(
                    AppEnv.Faults.Fatal("An invalid configuration file was passed on the command line"));

            var names = new List<string>();

            for (var i = 1; i < cmdArgsWithoutOptions.Count; i++)
            {
                var name = Path.GetFileName(cmdArgsWithoutOptions[i]);
                if (!string.IsNullOrWhiteSpace(name))
                    names.Add(name);
            }

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
                if (retval.ShowHelp.HasValue)
                    continue;
                retval = retval with { ShowHelp = true };
            }
            else if (MatchesOneOf(options, "V", current, "-v", "--version"))
            {
                if (retval.ShowVersion.HasValue)
                    continue;
                retval = retval with { ShowVersion = true };
            }
            else if (MatchesOneOf(options, "R", current, "-r", "--repository-root"))
            {
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
                i++; // eat the next argument
                
                if (retval.DeploymentBundle is not null)
                    continue;
                
                if (string.IsNullOrEmpty(next))
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The value for -o or --output-file is missing"));

                var deploymentBundle = new FileInfo(Path.GetFullPath(next));
                
                if (deploymentBundle.Directory is null || !deploymentBundle.Directory.Exists)
                    throw new VigoFatalException(AppEnv.Faults.Fatal($"The value for -o or --output-file must be file in an existing directory"));

                retval = retval with { DeploymentBundle = deploymentBundle };
            }
            else if (MatchesOneOf(options, "T", current, "-t", "--targets"))
            {
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
                throw new VigoFatalException(AppEnv.Faults.Fatal($"Unknown command line option {current}"));
            }
            else
            {
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