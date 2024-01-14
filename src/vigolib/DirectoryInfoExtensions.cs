using System.Security;
using Serilog;

namespace vigolib;

internal static class DirectoryInfoExtensions
{
    internal static bool CreateFolderIfMissing(this DirectoryInfo directoryInfo)
    {
        try
        {
            directoryInfo.Refresh();

            if (directoryInfo is { Exists: false, Parent.Exists: true })
            {
                directoryInfo.Create();
            }                
                
            directoryInfo.Refresh();
            
            return directoryInfo.Exists;
        }
        catch (SecurityException ex)
        {
            Log.Debug(ex, "Cannot make sure the directory exists {Directory}", directoryInfo.FullName);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Debug(ex, "Cannot make sure the directory exists {Directory}", directoryInfo.FullName);
            return false;
        }
        catch (DirectoryNotFoundException ex)
        {
            Log.Debug(ex, "Cannot make sure the directory exists {Directory}", directoryInfo.FullName);
            return false;
        }
        catch (IOException ex)
        {
            Log.Debug(ex, "Cannot make sure the directory exists {Directory}", directoryInfo.FullName);
            return false;
        }
    }
    
    internal static bool PurgeContents(this DirectoryInfo directoryInfo)
    {
        try
        {
            directoryInfo.Refresh();

            if (!directoryInfo.Exists)
                return false;

            foreach (var subdir in directoryInfo.EnumerateDirectories())
            {
                subdir.Delete(true);
            }

            foreach (var file in directoryInfo.EnumerateFiles())
            {
                file.Delete();
            }
            
            directoryInfo.Refresh();
            
            return !directoryInfo.GetFileSystemInfos().Any();
        }
        catch (SecurityException ex)
        {
            Log.Debug(ex, "Cannot purge the directory {Directory}", directoryInfo.FullName);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Debug(ex, "Cannot purge the directory {Directory}", directoryInfo.FullName);
            return false;
        }
        catch (DirectoryNotFoundException ex)
        {
            Log.Debug(ex, "Cannot purge the directory {Directory}", directoryInfo.FullName);
            return false;
        }
        catch (IOException ex)
        {
            Log.Debug(ex, "Cannot purge the directory {Directory}", directoryInfo.FullName);
            return false;
        }
    }
}