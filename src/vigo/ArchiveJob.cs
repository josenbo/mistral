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
        
        tarball.Save(new FileInfo(archiveFilePath));

        return true;
    }
}