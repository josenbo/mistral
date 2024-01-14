using JetBrains.Annotations;
using Serilog;
using vigocfg;
using vigoftg;

namespace vigolib;

[PublicAPI]
public class VigoJob : IVigoJob
{
    public IVigoJobResult Run()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        var retval = new VigoJobResult() { Success = false };
        
        stopwatch.Start();
        
        Log.Information("Preparing the deployment on {StagingEnvironmentName} ({StagingEnvironmentKey})", 
            _config.StagingEnvironment.Name,
            _config.StagingEnvironment.Key);

        try
        {
            var worklist = new List<IWork>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var appUser in _config.AppUsers.Where(s => s.TargetFolder is not null))
            {
                worklist.Add(new AppUserWork(appUser, _config, _nameParser));
            }
            
            if (_config.DatabaseScripts.TargetFolder is not null)
                worklist.Add(new DatabaseMigrationWork(_config.DatabaseScripts, _config, _nameParser));

            if (worklist.Any())
            {
                foreach (var work in worklist.Where(work => !(work.Prepare() && work.Execute() && work.Finish())))
                {
                    Log.Error("The task {TaskName} ran into problems and the deployment cannot be completed",
                        work.Name);

                    return retval;
                }

                retval.Success = true;

                stopwatch.Stop();

                Log.Information("{StagingEnvironment} is ready for deployment (Duration: {DurationMsec:#,##0} msec)", 
                    _config.StagingEnvironment.Key,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                retval.Success = false;
                stopwatch.Stop();
                Log.Error("No runnable tasks in {StagingEnvironment}", _config.StagingEnvironment.Key);
            }
            
            return retval;
        }
        catch (Exception e)
        {
            Log.Error(e, "Aborting program execution due to an unhandled exception");
            stopwatch.Stop();
            return retval;
        }
    }

    public VigoJob(IVigoConfig config, INameParser nameParser)
    {
        _config = config;
        _nameParser = nameParser;
    }

    private readonly IVigoConfig _config;
    private readonly INameParser _nameParser;
}