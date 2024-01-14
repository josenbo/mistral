using System.Collections.ObjectModel;
using System.Security;
using Serilog;
using vigocfg;

namespace vigolib;

internal class TargetBundle
{
    internal bool TransformAndProvisionAllFiles()
    {
        Log.Debug("Transformation and provisioning of files started");

        if (! PreExecutionChecks(_targetFiles.Values))
        {
            Log.Error("Pre-transformation checks failed");
            return false;
        }

        if (!TransformAndProvisionForFolders(GetGroupedTargetFilesForFolder()))
        {
            Log.Error("Failed to prepare folders for deployment");
            return false;
        }

        if (!TransformAndProvisionForDatabase(GetGroupedTargetFilesForDatabase()))
        {
            Log.Error("Failed to prepare database migrations for deployment");
            return false;
        }
        
        // ReSharper disable once InvertIf
        if (! PostExecutionChecks(_targetFiles.Values))
        {
            Log.Error("Post-transformation checks failed");
            return false;
        }

        Log.Debug("Transformation and provisioning of files successfully completed");
        
        return true;
    }

    internal IReadOnlyDictionary<TargetFileForFolderGroupKey, IReadOnlyCollection<TargetFileForFolder>> GetGroupedTargetFilesForFolder()
    {
        var retval = new Dictionary<TargetFileForFolderGroupKey, IReadOnlyCollection<TargetFileForFolder>>();

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var group in _targetFiles.Values.OfType<TargetFileForFolder>()
                     .GroupBy(g => g.FileGroup)
                     .Select(e => new { GroupName = e.Key, Items = e }))
        {
            retval.Add(
                new TargetFileForFolderGroupKey(
                    FolderGroupName: group.GroupName
                ), 
                new ReadOnlyCollection<TargetFileForFolder>(group.Items.ToList())
            );
        }

        return retval;
    }

    internal IReadOnlyDictionary<TargetFileForDatabaseGroupKey, IReadOnlyCollection<TargetFileForDatabase>> GetGroupedTargetFilesForDatabase()
    {
        var retval = new Dictionary<TargetFileForDatabaseGroupKey, IReadOnlyCollection<TargetFileForDatabase>>();

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var group in _targetFiles.Values.OfType<TargetFileForDatabase>()
                     .GroupBy(g => new { SchemaName = g.SchemaName, DatabaseObjectType = g.DatabaseObjectType } )
                     .Select(e => new { GroupKey = e.Key, Items = e }))
        {
            retval.Add( 
                new TargetFileForDatabaseGroupKey(
                    SchemaName: group.GroupKey.SchemaName, 
                    DatabaseObjectType: group.GroupKey.DatabaseObjectType
                ), 
                new ReadOnlyCollection<TargetFileForDatabase>(group.Items.ToList())
            );
        }

        return retval;
    }
    
    internal TargetFileForFolder AppendFromUserDeployment(FileInfo source, FileInfo target, string fileGroup, ITransferRule rule, IEnumerable<string> tags)
    {
        var targetFile = new TargetFileForFolder(
            source,
            target,
            rule.SourceFileType,
            rule.SourceEncoding,
            rule.TargetFilePermission,
            rule.TargetEncoding,
            rule.TargetLineEnding,
            rule.AppendFinalNewline,
            tags,
            fileGroup);
        
        _targetFiles.Add(targetFile.Key, targetFile);

        Log.Debug("Added target file {Targetile}", targetFile.Key);
        
        return targetFile;
    }

    private readonly Dictionary<string, TargetFile> _targetFiles = new(StringComparer.Ordinal);

    private static bool TransformAndProvision(TargetFileForFolder targetFile) => TransformAndProvisionGeneric(targetFile);
    
    private static bool TransformAndProvision(TargetFileForDatabase targetFile) => TransformAndProvisionGeneric(targetFile);
    
    private static bool TransformAndProvisionGeneric(TargetFile targetFile)
    {
        try
        {
            targetFile.Target.Refresh();

            if (targetFile.Target.Directory is null)
            {
                Log.Error("Cannot copy source file {SourceFileName} because the target file has no parent folder. Parameters were {TargetFileInstance}",
                    targetFile.Source.Name,
                    targetFile);
            
                return false;
            }
        
            if (!targetFile.Target.Directory.Exists)
                targetFile.Target.Directory.Create();

            if (targetFile.SourceFileType == FileTypeEnum.BinaryFile)
            {
                targetFile.Source.CopyTo(targetFile.Target.FullName, false);
                return true;
            }

            var text = File.ReadAllText(targetFile.Source.FullName, targetFile.SourceEncoding.ToEncoding());

            text = text
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");
                
            if (targetFile.TargetLineEnding != LineEndingEnum.LF)
                text = text.Replace("\n", targetFile.TargetLineEnding.ToNewlineSequence());

            if (targetFile.AppendFinalNewline && !text.EndsWith(targetFile.TargetLineEnding.ToNewlineSequence()))
                text = text + targetFile.TargetLineEnding.ToNewlineSequence();
            
            File.WriteAllText(targetFile.Target.FullName, text, targetFile.TargetEncoding.ToEncoding());

            targetFile.CopyAndTransformComplete = true;

            return true;
        }
        catch (SecurityException e)
        {
            Log.Error(e,
                "Failed to copy the source file {SourceFileName} due to an exception. Parameters were {TargetFileInstance}",
                targetFile.Source.Name,
                targetFile);

            return false;
        }
        catch (NotSupportedException e)
        {
            Log.Error(e,
                "Failed to copy the source file {SourceFileName} due to an exception. Parameters were {TargetFileInstance}",
                targetFile.Source.Name,
                targetFile);

            return false;
        }
        catch (UnauthorizedAccessException e)
        {
            Log.Error(e,
                "Failed to copy the source file {SourceFileName} due to an exception. Parameters were {TargetFileInstance}",
                targetFile.Source.Name,
                targetFile);

            return false;
        }
        catch (IOException e)
        {
            Log.Error(e,
                "Failed to copy the source file {SourceFileName} due to an exception. Parameters were {TargetFileInstance}",
                targetFile.Source.Name,
                targetFile);

            return false;
        }
        catch (ArgumentNullException e)
        {
            Log.Error(e,
                "Failed to copy the source file {SourceFileName} due to an exception. Parameters were {TargetFileInstance}",
                targetFile.Source.Name,
                targetFile);

            return false;
        }
    }

    private static bool PreExecutionChecks(IEnumerable<TargetFile> targetFiles)
    {
        return targetFiles.All(e => e.CheckBeforeCopy());
    }

    private static bool TransformAndProvisionForFolders(IReadOnlyDictionary<TargetFileForFolderGroupKey, IReadOnlyCollection<TargetFileForFolder>> groupedFiles)
    {
        foreach (var kv in groupedFiles)
        {
            if (kv.Value.All(TransformAndProvision)) continue;
            
            Log.Error("Failed to copy a file of the the folder group {GroupKey}", kv.Key);
            return false;
        }
        return true;       
    }

    private static bool TransformAndProvisionForDatabase(IReadOnlyDictionary<TargetFileForDatabaseGroupKey, IReadOnlyCollection<TargetFileForDatabase>> groupedFiles)
    {
        foreach (var kv in groupedFiles)
        {
            if (kv.Value.All(TransformAndProvision)) continue;
            
            Log.Error("Failed to copy a file of the the database group {GroupKey}", kv.Key);
            return false;
        }
        return true;       
    }
    
    private static bool PostExecutionChecks(IEnumerable<TargetFile> targetFiles)
    {
        return targetFiles.All(e => e.CheckAfterCopy());
    }
}