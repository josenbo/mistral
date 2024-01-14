using Serilog;
using vigocfg;
using vigoftg;

namespace vigolib;

internal class DatabaseMigrationWork: IWork
{
    public string Name { get; }
    internal DatabaseMigrationWork(IDatabaseScripts databaseScripts, IVigoConfig config, INameParser nameParser)
    {
        _databaseScripts = databaseScripts;
        _nameParser = nameParser;
        _config = config;
        Name = "Prepare Database Deployment";
    }

    public bool Prepare()
    {
        Log.Information("Preparing the database deployment");
        Log.Debug("Entering the phase {PhaseName} of the task {TaskName}", nameof(Prepare), Name);

        Log.Debug("Leaving the phase {PhaseName} of the task {TaskName}", nameof(Prepare), Name);
        return true;
    }

    public bool Execute()
    {
        Log.Debug("Entering the phase {PhaseName} of the task {TaskName}", nameof(Execute), Name);

        Log.Debug("Leaving the phase {PhaseName} of the task {TaskName}", nameof(Execute), Name);
        return true;
    }

    public bool Finish()
    {
        Log.Debug("Entering the phase {PhaseName} of the task {TaskName}", nameof(Finish), Name);

        Log.Debug("Leaving the phase {PhaseName} of the task {TaskName}", nameof(Finish), Name);
        return true;
    }

    private readonly TargetBundle _targetBundle = new();
    private readonly IDatabaseScripts _databaseScripts;
    private readonly IVigoConfig _config;
    private readonly INameParser _nameParser;
}