using System.Data.SqlTypes;
using Serilog;
using vigoarchive;
using vigorule;

namespace vigo;

internal abstract class JobRunner
{
    // todo: set Success in derived classes
    public bool Success { get; protected set; }
    public abstract bool Prepare();
    public abstract bool Run();
    public abstract void CleanUp();
    
    protected static bool BuildTarball(IRepositoryReader reader, FileInfo outputFile, IReadOnlyList<string> filterByTargets)
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