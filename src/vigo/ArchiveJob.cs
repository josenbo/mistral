using vigoarchive;

namespace vigo;

public class ArchiveJob : IJob
{
    public bool Run()
    {
        const string baseDirPath = @"I:\INES-Github\_workspaces_\vigo\CompressedFolders\Data";
        const string archiveFilePath = @"I:\INES-Github\_workspaces_\vigo\CompressedFolders\Out\archive.tar.gz";

        var tarball = new Tarball(baseDirPath, "ines_sandbox_3");
        
        foreach (var fi in new DirectoryInfo(baseDirPath).EnumerateFiles("*.*", SearchOption.AllDirectories))
        {
            tarball.AddFile(fi); 
        }

        tarball.GetFolder("Folder_1", true, out _);
        tarball.GetFolder("Folder_2/Folder_2_2", true, out _);
        tarball.GetFolder("b/c/d/ende", true, out _);
        tarball.GetFolder("b/c/x/q/ende", true, out _);
        
        tarball.Save(new FileInfo(archiveFilePath));

        return true;
    }
}