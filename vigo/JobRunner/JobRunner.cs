using Serilog;
using vigoarchive;
using vigobase;
using vigorule;

namespace vigo;

internal abstract class JobRunner
{
    public bool Success { get; private set; }

    public bool Run()
    {
        Success = DoPrepare();

        try
        {
            if (Success)
                Success = DoRun();
        }
        catch (Exception ex) when (ex is not VigoFatalException)
        {
            throw new VigoFatalException(
                message: AppEnv.Faults.Fatal(
                    faultKey: "FX672",
                    supportInfo: $"Try to handle this exception at the origin. Caught unhandled {ex.GetType().Name} with message {ex.Message}",
                    message: JobRunnerFailureMessage), 
                innerException: ex);
        }
        finally
        {
            DoCleanUp();
        }

        return Success;
    }

    protected abstract bool DoPrepare();
    protected abstract bool DoRun();
    protected abstract void DoCleanUp();
    protected abstract string JobRunnerFailureMessage { get; }
    
    
    protected static bool BuildTarball(IRepositoryReader reader, FileInfo outputFile, IReadOnlyList<string> filterByTargets, bool writeCheckTarget)
    {
        try
        {
            var directoryTimestamp = DateTimeOffset.Now;


            var targets = 0 < filterByTargets.Count
                ? reader
                    .Targets()
                    .Where(t => filterByTargets.Contains(t, StringComparer.InvariantCultureIgnoreCase))
                    .ToList()
                : reader
                    .Targets()
                    .ToList();

            var requests = new List<TarItem>();
            
            foreach (var target in targets)
            {
                if (target == "_check_target_" && !writeCheckTarget)
                    continue;
                
                // ReSharper disable LoopCanBeConvertedToQuery

                foreach (var transformation in reader.FinalItems<IFinalFileHandling>(target))
                {
                    requests.Add(new TarItemFile(
                        TarRelativePath: Path.Combine(target, reader.GetTopLevelRelativePath(transformation.TargetFile)),
                        TransformedContent: transformation.CheckedAndTransformedTemporaryFile,
                        FileMode: transformation.FilePermission.ComputeUnixFileMode(reader.DefaultHandling.StandardModeForFiles),
                        ModificationTime: DateTimeOffset.FromFileTime(transformation.SourceFile.LastWriteTime.ToFileTime())
                        ));
                }

                foreach (var transformation in reader.FinalItems<IFinalDirectoryHandling>(target))
                {
                    requests.Add(new TarItemDirectory(
                        TarRelativePath: Path.Combine(target, reader.GetTopLevelRelativePath(transformation.SourceDirectory)),
                        FileMode: reader.DefaultHandling.StandardModeForDirectories,
                        ModificationTime: directoryTimestamp
                        ));
                }
                
                // ReSharper restore LoopCanBeConvertedToQuery
            }

            var tarball = new Tarball()
            {
                DirectoryFileMode = reader.DefaultHandling.StandardModeForDirectories
            };

            foreach (var request in requests)
            {
                tarball.AddItem(request);
            }
        
            tarball.Save(outputFile);
            
            return true;
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to deploy repository files to a tarball. The tarball file might be incomplete and will be deleted, if it exists");
            Console.Error.WriteLine("Fatal: the program terminates prematurely due to an unhandled error");

            try
            {
                outputFile.Refresh();
                if (outputFile.Exists)
                    outputFile.Delete();
            }
            catch (Exception)
            {
                // ignore errors during cleanup
            }

            return false;
        }
    }
}